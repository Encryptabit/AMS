using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ams.Core;

public sealed record ProcessResult(int ExitCode, string StdOut, string StdErr);

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(string fileName, string arguments, CancellationToken ct = default);
}

public sealed class DefaultProcessRunner : IProcessRunner
{
    public async Task<ProcessResult> RunAsync(string fileName, string arguments, CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        using var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
        var so = new StringBuilder();
        var se = new StringBuilder();
        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        p.OutputDataReceived += (_, e) => { if (e.Data != null) so.AppendLine(e.Data); };
        p.ErrorDataReceived += (_, e) => { if (e.Data != null) se.AppendLine(e.Data); };
        p.Exited += (_, __) => tcs.TrySetResult(p.ExitCode);

        if (!p.Start())
            throw new InvalidOperationException($"Failed to start process '{fileName}'.");
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();

        await using var _ = ct.Register(() => { try { if (!p.HasExited) p.Kill(); } catch { /* ignored */ } });
        var code = await tcs.Task.ConfigureAwait(false);
        return new ProcessResult(code, so.ToString(), se.ToString());
    }
}

