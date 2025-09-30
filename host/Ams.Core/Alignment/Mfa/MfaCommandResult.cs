namespace Ams.Core.Alignment.Mfa;

public sealed record MfaCommandResult(int ExitCode, IReadOnlyList<string> StdOut, IReadOnlyList<string> StdErr);
