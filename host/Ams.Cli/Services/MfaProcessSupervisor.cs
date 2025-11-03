using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Ams.Core.Alignment.Mfa;
using Ams.Core.Common;

namespace Ams.Cli.Services;

/// <summary>
/// Maintains a warm PowerShell session with the MFA conda environment activated so commands can
/// be dispatched without incurring startup overhead on each invocation.
/// </summary>
internal static class MfaProcessSupervisor
{
    private const string ReadyToken = "__MFA_READY__";
    private const string ExitToken = "__MFA_EXIT__";
    private const string QuitToken = "__QUIT__";

    private static readonly SemaphoreSlim CommandGate = new(1, 1);
    private static readonly object StartLock = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static Process? _process;
    private static StreamWriter? _stdin;
    private static Channel<ProcessLine>? _lineChannel;
    private static Task? _stdoutPump;
    private static Task? _stderrPump;
    private static CancellationTokenSource? _pumpCts;
    private static bool _isReady;
    private static string? _scriptPath;
    private static int _activePumpCount;
    private static Task? _startTask;

    private enum StreamKind
    {
        StdOut,
        StdErr
    }

    private readonly record struct ProcessLine(StreamKind Kind, string Line);

    private readonly record struct Payload(string Command, string? WorkingDir);

    internal static void RegisterForShutdown()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Shutdown();
        Console.CancelKeyPress += (_, _) => Shutdown();
    }

    internal static void TriggerBackgroundWarmup()
    {
        Task.Run(async () =>
        {
            try
            {
                await EnsureStartedAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Debug("MFA warmup failed: {0}", ex);
            }
        });
    }

    internal static Task EnsureReadyAsync(CancellationToken cancellationToken)
    {
        return EnsureStartedAsync(cancellationToken);
    }

    internal static async Task<MfaCommandResult> RunAsync(
        string subcommand,
        string? args,
        string? workingDirectory,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subcommand))
        {
            throw new ArgumentException("Subcommand must be provided", nameof(subcommand));
        }

        await CommandGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await EnsureStartedAsync(cancellationToken).ConfigureAwait(false);

            var command = BuildCommand(subcommand, args);
            var payload = new Payload(command, NormalizeWorkingDirectory(workingDirectory));
            var payloadJson = JsonSerializer.Serialize(payload, JsonOptions);

            Log.Debug("MFA> {Command}", command);

            await _stdin!.WriteLineAsync(payloadJson.AsMemory(), cancellationToken).ConfigureAwait(false);
            await _stdin.FlushAsync().ConfigureAwait(false);

            var stdout = new List<string>();
            var stderr = new List<string>();
            var exitCode = await WaitForCommandCompletionAsync(stdout, stderr, cancellationToken)
                .ConfigureAwait(false);

            return new MfaCommandResult(command, exitCode, stdout, stderr);
        }
        finally
        {
            CommandGate.Release();
        }
    }

    private static Task EnsureStartedAsync(CancellationToken cancellationToken)
    {
        Task startTask;

        lock (StartLock)
        {
            if (_process is { HasExited: false } && _isReady)
            {
                return Task.CompletedTask;
            }

            if (_startTask is null || _startTask.IsCompleted)
            {
                _startTask = StartProcessAsync();
            }

            startTask = _startTask;
        }

        return startTask.WaitAsync(cancellationToken);
    }

    private static async Task StartProcessAsync()
    {
        lock (StartLock)
        {
            if (_process is { HasExited: false } && _isReady)
            {
                return;
            }

            TearDownProcess();

            EnsureBootstrapScript();

            var psi = new ProcessStartInfo(ResolvePwshExecutable())
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            psi.ArgumentList.Add("-ExecutionPolicy");
            psi.ArgumentList.Add("ByPass");
            psi.ArgumentList.Add("-NoLogo");
            psi.ArgumentList.Add("-NoProfile");
            psi.ArgumentList.Add("-File");
            psi.ArgumentList.Add(_scriptPath!);

            _process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            _process.Start();
            _stdin = _process.StandardInput;
            if (_stdin is not null)
            {
                _stdin.AutoFlush = true;
            }

            _pumpCts = new CancellationTokenSource();
            _lineChannel = Channel.CreateUnbounded<ProcessLine>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

            _activePumpCount = 2;
            _stdoutPump = Task.Run(() => PumpAsync(_process.StandardOutput, StreamKind.StdOut, _pumpCts.Token));
            _stderrPump = Task.Run(() => PumpAsync(_process.StandardError, StreamKind.StdErr, _pumpCts.Token));
            _isReady = false;
        }

        try
        {
            await WaitForReadyAsync(CancellationToken.None).ConfigureAwait(false);
            _isReady = true;
        }
        finally
        {
            lock (StartLock)
            {
                _startTask = null;
            }
        }
    }

    private static void EnsureBootstrapScript()
    {
        if (_scriptPath is not null && File.Exists(_scriptPath))
        {
            return;
        }

        var bootstrap = ResolveBootstrapSequence();
        var script = BuildSupervisorScript(bootstrap);

        var tempPath = Path.Combine(Path.GetTempPath(), "ams-mfa-supervisor.ps1");
        File.WriteAllText(tempPath, script, Encoding.UTF8);
        _scriptPath = tempPath;
    }

    private static async Task<int> WaitForCommandCompletionAsync(
        List<string> stdout,
        List<string> stderr,
        CancellationToken cancellationToken)
    {
        var channel = _lineChannel ?? throw new InvalidOperationException("Supervisor channel not initialized");

        while (await channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            while (channel.Reader.TryRead(out var line))
            {
                if (line.Kind == StreamKind.StdOut)
                {
                    if (line.Line.StartsWith(ExitToken, StringComparison.Ordinal))
                    {
                        if (int.TryParse(line.Line.AsSpan(ExitToken.Length), out var code))
                        {
                            return code;
                        }

                        return -1;
                    }

                    stdout.Add(line.Line);
                }
                else
                {
                    stderr.Add(line.Line);
                }
            }
        }

        throw new InvalidOperationException("MFA supervisor terminated unexpectedly without exit token");
    }

    private static async Task WaitForReadyAsync(CancellationToken cancellationToken)
    {
        var channel = _lineChannel ?? throw new InvalidOperationException("Supervisor channel not initialized");

        while (await channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            while (channel.Reader.TryRead(out var line))
            {
                if (line.Kind == StreamKind.StdOut && string.Equals(line.Line, ReadyToken, StringComparison.Ordinal))
                {
                    Log.Debug("MFA environment ready");
                    return;
                }

                if (line.Kind == StreamKind.StdErr)
                {
                    Log.Debug("mfa! {Line}", line.Line);
                }
                else
                {
                    Log.Debug("mfa> {Line}", line.Line);
                }
            }
        }

        throw new InvalidOperationException("MFA supervisor failed to signal readiness");
    }

    private static async Task PumpAsync(StreamReader reader, StreamKind kind, CancellationToken cancellationToken)
    {
        var channel = _lineChannel ?? throw new InvalidOperationException("Supervisor channel not initialized");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line is null)
                {
                    break;
                }

                await channel.Writer.WriteAsync(new ProcessLine(kind, line), cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown.
        }
        catch (Exception ex)
        {
            Log.Debug("MFA output pump faulted: {0}", ex);
        }
        finally
        {
            if (Interlocked.Decrement(ref _activePumpCount) <= 0)
            {
                channel.Writer.TryComplete();
            }
        }
    }

    internal static void Shutdown()
    {
        lock (StartLock)
        {
            if (_stdin is not null)
            {
                try
                {
                    Log.Debug("Stopping MFA process...");
                    _stdin.WriteLine(QuitToken);
                    _stdin.Flush();
                }
                catch (Exception ex)
                {
                    Log.Debug("Failed to signal MFA shutdown: {0}", ex.Message);
                }
            }

            TearDownProcess();
        }
    }

    private static void TearDownProcess()
    {
        try
        {
            _pumpCts?.Cancel();
        }
        catch
        {
            // ignore
        }

        _pumpCts?.Dispose();
        _pumpCts = null;
        _stdoutPump = null;
        _stderrPump = null;
        _activePumpCount = 0;

        _lineChannel?.Writer.TryComplete();
        _lineChannel = null;

        _startTask = null;
        _isReady = false;

        if (_process is { HasExited: false })
        {
            try
            {
                _process.Kill(true);
            }
            catch
            {
                // ignore
            }
        }

        if (_process is not null)
        {
            try
            {
                if (!_process.HasExited)
                {
                    _process.WaitForExit(2000);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Error while waiting for MFA process exit: {0}", ex.Message);
            }
            finally
            {
                Log.Debug("MFA process terminated.");
                _process.Dispose();
                _process = null;
            }
        }

        _stdin = null;
        _isReady = false;

        if (_scriptPath is not null)
        {
            try
            {
                if (File.Exists(_scriptPath))
                {
                    File.Delete(_scriptPath);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Failed to delete MFA bootstrap script {Path}: {Message}", _scriptPath, ex.Message);
            }
            finally
            {
                _scriptPath = null;
            }
        }
    }

    private static string BuildCommand(string subcommand, string? args)
    {
        var commandBuilder = new StringBuilder("mfa ");
        commandBuilder.Append(subcommand.Trim());

        if (!string.IsNullOrWhiteSpace(args))
        {
            commandBuilder.Append(' ');
            commandBuilder.Append(args.Trim());
        }

        return commandBuilder.ToString();
    }

    private static string? NormalizeWorkingDirectory(string? workingDirectory)
    {
        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            return null;
        }

        try
        {
            return Path.GetFullPath(workingDirectory);
        }
        catch
        {
            return workingDirectory;
        }
    }

    private static string ResolvePwshExecutable()
    {
        var fromEnv = Environment.GetEnvironmentVariable("AMS_MFA_PWSH");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "pwsh.exe";
        }

        return "pwsh";
    }

    private static string ResolveBootstrapSequence()
    {
        var fromEnv = Environment.GetEnvironmentVariable("AMS_MFA_BOOTSTRAP");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv.Replace("\r\n", "\n");
        }

        // Default sequence discovered during manual setup.
        return string.Join(
            Environment.NewLine,
            "& 'C:/Users/Jacar/miniconda3/shell/condabin/conda-hook.ps1'",
            "conda activate 'C:/Users/Jacar/miniconda3'",
            "conda activate aligner");
    }

    private static string BuildSupervisorScript(string bootstrap)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Auto-generated by AMS to manage MFA environment");
        builder.AppendLine("$ErrorActionPreference = 'Continue'");
        builder.AppendLine("$bootstrap = @'");
        builder.AppendLine(bootstrap);
        builder.AppendLine("'@");
        builder.AppendLine("try {");
        builder.AppendLine("    Invoke-Expression $bootstrap");
        builder.AppendLine("} catch {");
        builder.AppendLine("    Write-Error $_");
        builder.AppendLine("}");
        builder.AppendLine("Write-Output '" + ReadyToken + "'");
        builder.AppendLine("while ($true) {");
        builder.AppendLine("    $line = [Console]::In.ReadLine()");
        builder.AppendLine("    if ($null -eq $line) { break }");
        builder.AppendLine("    if ($line -eq '" + QuitToken + "') { break }");
        builder.AppendLine("    if ([string]::IsNullOrWhiteSpace($line)) { continue }");
        builder.AppendLine("    try {");
        builder.AppendLine("        $payload = $line | ConvertFrom-Json");
        builder.AppendLine("    } catch {");
        builder.AppendLine("        Write-Error ('Invalid JSON payload: {0}' -f $line)");
        builder.AppendLine("        Write-Output '" + ExitToken + "1'");
        builder.AppendLine("        continue");
        builder.AppendLine("    }");
        builder.AppendLine("    $command = $payload.command");
        builder.AppendLine("    $workingDir = $payload.workingDir");
        builder.AppendLine("    if ($workingDir) {");
        builder.AppendLine("        Set-Location $workingDir");
        builder.AppendLine("    }");
        builder.AppendLine("    $global:LASTEXITCODE = 0");
        builder.AppendLine("    try {");
        builder.AppendLine("        Invoke-Expression $command");
        builder.AppendLine("        $exitCode = if ($LASTEXITCODE) { $LASTEXITCODE } else { 0 }");
        builder.AppendLine("        Write-Output ('" + ExitToken + "{0}' -f $exitCode)");
        builder.AppendLine("    } catch {");
        builder.AppendLine("        Write-Error $_");
        builder.AppendLine("        $exitCode = if ($LASTEXITCODE) { $LASTEXITCODE } else { 1 }");
        builder.AppendLine("        Write-Output ('" + ExitToken + "{0}' -f $exitCode)");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }
}
