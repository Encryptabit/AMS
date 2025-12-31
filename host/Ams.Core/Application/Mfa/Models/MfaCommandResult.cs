namespace Ams.Core.Application.Mfa.Models;

public sealed record MfaCommandResult(
    string Command,
    int ExitCode,
    IReadOnlyList<string> StdOut,
    IReadOnlyList<string> StdErr);