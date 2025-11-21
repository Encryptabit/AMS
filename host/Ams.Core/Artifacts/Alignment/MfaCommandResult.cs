namespace Ams.Core.Artifacts.Alignment;

public sealed record MfaCommandResult(string Command, int ExitCode, IReadOnlyList<string> StdOut, IReadOnlyList<string> StdErr);
