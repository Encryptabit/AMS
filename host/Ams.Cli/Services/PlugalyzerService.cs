using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ams.Cli.Services;

internal static class PlugalyzerService
{
    private const string EnvExecutable = "AMS_PLUGALYZER_EXE";
    private static readonly object CacheLock = new();
    private static string? _cachedExecutable;

    internal static string ResolveExecutable()
    {
        if (_cachedExecutable is not null)
        {
            return _cachedExecutable;
        }

        lock (CacheLock)
        {
            if (_cachedExecutable is not null)
            {
                return _cachedExecutable;
            }

            var fromEnv = Environment.GetEnvironmentVariable(EnvExecutable);
            if (!string.IsNullOrWhiteSpace(fromEnv) && File.Exists(fromEnv))
            {
                _cachedExecutable = Path.GetFullPath(fromEnv);
                return _cachedExecutable;
            }

            var candidate = ProbeForExecutable();
            if (candidate is null)
            {
                throw new FileNotFoundException(
                    $"Unable to locate Plugalyzer. Set {EnvExecutable} to the executable path or ensure host/ExtTools/Plugalyzer.exe is present.");
            }

            _cachedExecutable = candidate;
            return _cachedExecutable;
        }
    }

    internal static async Task<int> RunAsync(
        IReadOnlyList<string> arguments,
        string? workingDirectory,
        CancellationToken cancellationToken,
        Action<string>? onStdOut = null,
        Action<string>? onStdErr = null)
    {
        var executable = ResolveExecutable();
        var psi = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (!string.IsNullOrWhiteSpace(workingDirectory))
        {
            psi.WorkingDirectory = workingDirectory;
        }

        foreach (var arg in arguments)
        {
            psi.ArgumentList.Add(arg);
        }

        Log.Debug("Launching Plugalyzer: {Exe} {Args}", executable, string.Join(' ', psi.ArgumentList));

        using var process = new Process
        {
            StartInfo = psi,
            EnableRaisingEvents = true
        };

        var stdoutTcs = new TaskCompletionSource<object?>();
        var stderrTcs = new TaskCompletionSource<object?>();

        onStdOut ??= line => Log.Debug("plugalyzer> {Line}", line);
        onStdErr ??= line => Log.Debug("plugalyzer! {Line}", line);

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is null)
            {
                stdoutTcs.TrySetResult(null);
            }
            else
            {
                onStdOut(e.Data);
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null)
            {
                stderrTcs.TrySetResult(null);
            }
            else
            {
                onStdErr(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await using var _ = cancellationToken.Register(() =>
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }
            }
            catch
            {
            }
        });

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        await Task.WhenAll(stdoutTcs.Task, stderrTcs.Task).ConfigureAwait(false);

        return process.ExitCode;
    }

    private static string? ProbeForExecutable()
    {
        var baseDir = AppContext.BaseDirectory;
        for (int i = 0; i < 8 && baseDir is not null; i++)
        {
            var candidate = Path.Combine(baseDir, "host", "ExtTools", "Plugalyzer.exe");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            candidate = Path.Combine(baseDir, "ExtTools", "Plugalyzer.exe");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            baseDir = Directory.GetParent(baseDir)?.FullName;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var defaultCandidate = Path.Combine(Environment.CurrentDirectory, "Plugalyzer.exe");
            if (File.Exists(defaultCandidate))
            {
                return defaultCandidate;
            }
        }

        return null;
    }
}

