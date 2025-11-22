using Ams.Core.Processors.Alignment.Mfa;

namespace Ams.Core.Artifacts.Alignment.Mfa;

public sealed record TextGridDocument(
    string SourcePath,
    DateTime ParsedAtUtc,
    IReadOnlyList<TextGridInterval> Intervals);