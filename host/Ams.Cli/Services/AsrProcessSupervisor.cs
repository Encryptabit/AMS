using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Asr;
using Ams.Core.Common;

namespace Ams.Cli.Services;

/// <summary>
/// Supervises a local asr-nemo host so CLI commands can assume a warm ASR endpoint.
/// Spawns the service on demand (via the repo's startup scripts) and tears it down when we own it.
/// </summary>
internal static class AsrProcessSupervisor
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly object ShutdownLock = new();

    private static Process? _process;
    private static bool _ownsProcess;
    private static string? _managedBaseUrl;
    private static string? _repoRoot;
    private static Task? _warmupTask;
    private static volatile SupervisorState _state;

    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan HealthInterval = TimeSpan.FromSeconds(2);

    private static readonly string DisableAutoStartEnv = "AMS_ASR_DISABLE_AUTOSTART";
    private static readonly string StartScriptEnv = "AMS_ASR_START_SCRIPT";
    private static readonly string PowerShellEnv = "AMS_ASR_POWERSHELL";
    private static readonly string PythonEnv = "AMS_ASR_PYTHON";

    private static bool NemoEnabled => AsrEngineConfig.IsNemo();

    internal static string StatusLabel
    {
        get
        {
            if (!NemoEnabled)
            {
                return "ASR:whisper";
            }

            return _state switch
            {
                SupervisorState.Ready => "ASR:ready",
                SupervisorState.Starting => "ASR:starting",
                SupervisorState.Faulted => "ASR:fault",
                _ => "ASR:idle"
            };
        }
    }

    internal static string StatusDescription
    {
        get
        {
            if (!NemoEnabled)
            {
                return "in-process";
            }

            return _state switch
            {
                SupervisorState.Ready => "ready",
                SupervisorState.Starting => "starting",
                SupervisorState.Faulted => "faulted",
                _ => "idle"
            };
        }
    }

    internal static string? BaseUrl => NemoEnabled ? _managedBaseUrl : null;

    internal static void RegisterForShutdown()
    {
        if (!NemoEnabled)
        {
            return;
        }

        AppDomain.CurrentDomain.ProcessExit += (_, _) => Shutdown();
        Console.CancelKeyPress += (_, _) => Shutdown();
    }

    internal static void TriggerBackgroundWarmup(string baseUrl)
    {
        if (!NemoEnabled)
        {
            _state = SupervisorState.Ready;
            return;
        }

        _managedBaseUrl = baseUrl;

        if (IsAutoStartDisabled())
        {
            Log.Debug("ASR auto-start disabled via {Env}", DisableAutoStartEnv);
            return;
        }

        lock (ShutdownLock)
        {
            if (_warmupTask is { IsCompleted: false })
            {
                return;
            }

            _warmupTask = Task.Run(async () =>
            {
                try
                {
                    await EnsureServiceReadyAsync(baseUrl, CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Debug("Background ASR warmup failed: {0}",ex);
                }
            });
        }
    }

    internal static async Task EnsureServiceReadyAsync(string baseUrl, CancellationToken cancellationToken)
    {
        if (!NemoEnabled)
        {
            _state = SupervisorState.Ready;
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        await Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _managedBaseUrl = baseUrl;

            if (await IsHealthyAsync(baseUrl, cancellationToken).ConfigureAwait(false))
            {
                _state = SupervisorState.Ready;
                return;
            }

            if (!IsLocalBaseUrl(baseUrl))
            {
                _state = SupervisorState.Faulted;
                throw new InvalidOperationException($"ASR endpoint {baseUrl} is unreachable and not local; cannot auto-start");
            }

            if (IsAutoStartDisabled())
            {
                _state = SupervisorState.Faulted;
                throw new InvalidOperationException("ASR service not running and auto-start disabled (set AMS_ASR_DISABLE_AUTOSTART=0 to enable)");
            }

            if (_process is { HasExited: false })
            {
                _state = SupervisorState.Starting;
                if (await WaitForHealthyAsync(baseUrl, cancellationToken).ConfigureAwait(false))
                {
                    _state = SupervisorState.Ready;
                    return;
                }

                Log.Debug("Existing managed ASR process did not become healthy; restarting");
                KillProcess();
            }

            StartManagedProcess();

            if (!await WaitForHealthyAsync(baseUrl, cancellationToken).ConfigureAwait(false))
            {
                _state = SupervisorState.Faulted;
                throw new InvalidOperationException("Timed out waiting for ASR service to report healthy state");
            }

            _state = SupervisorState.Ready;
        }
        finally
        {
            Gate.Release();
        }
    }

    internal static void Shutdown()
    {
        if (!NemoEnabled)
        {
            return;
        }

        lock (ShutdownLock)
        {
            if (_process is null)
            {
                return;
            }

            try
            {
                if (_ownsProcess && !_process.HasExited)
                {
                    Log.Debug("Stopping managed ASR process (PID {Pid})", _process.Id);
                    _process.Kill(true);
                    if (!_process.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds))
                    {
                        Log.Debug("ASR process did not exit within timeout");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Error while stopping ASR process {0}", ex);
            }
            finally
            {
                _process.Dispose();
                _process = null;
                _ownsProcess = false;
                _state = SupervisorState.Idle;
            }
        }
    }

    private static async Task<bool> IsHealthyAsync(string baseUrl, CancellationToken cancellationToken)
    {
        if (!NemoEnabled)
        {
            return true;
        }

        try
        {
            using var client = new AsrClient(baseUrl);
            return await client.IsHealthyAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> WaitForHealthyAsync(string baseUrl, CancellationToken cancellationToken)
    {
        if (!NemoEnabled)
        {
            _state = SupervisorState.Ready;
            return true;
        }

        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < StartupTimeout)
        {
            if (await IsHealthyAsync(baseUrl, cancellationToken).ConfigureAwait(false))
            {
                return true;
            }

            await Task.Delay(HealthInterval, cancellationToken).ConfigureAwait(false);
        }

        return false;
    }

    private static void StartManagedProcess()
    {
        var startInfo = BuildStartInfo();
        if (startInfo is null)
        {
            _state = SupervisorState.Faulted;
            throw new InvalidOperationException("Unable to locate asr-nemo startup script. Set AMS_ASR_START_SCRIPT to override.");
        }

        Log.Debug("Starting asr-nemo via {File} {Args}", startInfo.FileName, startInfo.Arguments);

        try
        {
            var process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    Log.Debug("asr-nemo: {Line}", e.Data);
                }
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    Log.Debug("asr-nemo: {Line}", e.Data);
                }
            };

            process.Exited += (_, _) =>
            {
                if (_ownsProcess)
                {
                    Log.Debug("Managed ASR process exited with code {ExitCode}", process.ExitCode);
                    _state = SupervisorState.Faulted;
                }
            };

            if (!process.Start())
            {
                throw new InvalidOperationException("Failed to start asr-nemo process");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            _process = process;
            _ownsProcess = true;
            _state = SupervisorState.Starting;
        }
        catch (Exception ex)
        {
            _state = SupervisorState.Faulted;
            throw new InvalidOperationException("Failed to launch asr-nemo startup script", ex);
        }
    }

    private static void KillProcess()
    {
        if (_process is null)
        {
            return;
        }

        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(true);
                _process.WaitForExit((int)TimeSpan.FromSeconds(5).TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to terminate existing ASR process: {0}", ex);
        }
        finally
        {
            _process.Dispose();
            _process = null;
            _ownsProcess = false;
        }
    }

    private static ProcessStartInfo? BuildStartInfo()
    {
        var explicitScript = Environment.GetEnvironmentVariable(StartScriptEnv);
        if (!string.IsNullOrWhiteSpace(explicitScript))
        {
            return CreateStartInfoForScript(explicitScript);
        }

        var repoRoot = ResolveRepoRoot();
        if (repoRoot is null)
        {
            return null;
        }

        var serviceDir = Path.Combine(repoRoot, "services", "asr-nemo");
        if (!Directory.Exists(serviceDir))
        {
            return null;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var scriptPath = Path.Combine(serviceDir, "start_service.ps1");
            if (!File.Exists(scriptPath))
            {
                return null;
            }

            var shell = ResolvePowerShell();
            var args = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"";
            return new ProcessStartInfo(shell, args)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = serviceDir
            };
        }
        else
        {
            var appPath = Path.Combine(serviceDir, "app.py");
            if (!File.Exists(appPath))
            {
                return null;
            }

            var python = Environment.GetEnvironmentVariable(PythonEnv);
            if (string.IsNullOrWhiteSpace(python))
            {
                python = "python3";
            }

            var args = $"\"{appPath}\"";
            return new ProcessStartInfo(python, args)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = serviceDir
            };
        }
    }

    private static ProcessStartInfo? CreateStartInfoForScript(string scriptPath)
    {
        if (!File.Exists(scriptPath))
        {
            Log.Debug("Configured ASR start script not found at {Path}", scriptPath);
            return null;
        }

        var extension = Path.GetExtension(scriptPath).ToLowerInvariant();
        if (extension == ".ps1" && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var shell = ResolvePowerShell();
            var args = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"";
            return new ProcessStartInfo(shell, args)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath)!
            };
        }

        if ((extension == ".bat" || extension == ".cmd") && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var args = $"/c \"\"{scriptPath}\"\"";
            return new ProcessStartInfo(Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe", args)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath)!
            };
        }

        if (extension == ".py")
        {
            var python = Environment.GetEnvironmentVariable(PythonEnv);
            if (string.IsNullOrWhiteSpace(python))
            {
                python = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "python" : "python3";
            }

            return new ProcessStartInfo(python, $"\"{scriptPath}\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath)!
            };
        }

        Log.Debug("Unsupported ASR start script extension: {Ext}", extension);
        return null;
    }

    private static string ResolvePowerShell()
    {
        var configured = Environment.GetEnvironmentVariable(PowerShellEnv);
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Prefer pwsh if available, fall back to Windows PowerShell.
            var pwshPath = TryFindOnPath("pwsh.exe");
            if (pwshPath is not null)
            {
                return pwshPath;
            }

            return TryFindOnPath("powershell.exe") ?? "powershell";
        }

        return "pwsh";
    }

    private static string? TryFindOnPath(string fileName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        foreach (var segment in path.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                continue;
            }

            var candidate = Path.Combine(segment.Trim(), fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static string? ResolveRepoRoot()
    {
        if (!string.IsNullOrEmpty(_repoRoot))
        {
            return _repoRoot;
        }

        var current = AppContext.BaseDirectory;

        for (int i = 0; i < 8 && current is not null; i++)
        {
            if (Directory.Exists(Path.Combine(current, "services")) && File.Exists(Path.Combine(current, "ProjectState.md")))
            {
                _repoRoot = current;
                return _repoRoot;
            }

            current = Directory.GetParent(current)?.FullName;
        }

        return null;
    }

    private static bool IsLocalBaseUrl(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
               || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
               || uri.Host.Equals("::1", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAutoStartDisabled()
    {
        var value = Environment.GetEnvironmentVariable(DisableAutoStartEnv);
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return value.Equals("1", StringComparison.OrdinalIgnoreCase)
               || value.Equals("true", StringComparison.OrdinalIgnoreCase)
               || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }

    private enum SupervisorState
    {
        Idle,
        Starting,
        Ready,
        Faulted
    }
}
