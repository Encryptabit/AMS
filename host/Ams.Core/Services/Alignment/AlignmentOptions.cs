namespace Ams.Core.Services.Alignment;

public sealed record AnchorComputationOptions
{
    public int NGram { get; init; } = 3;
    public int TargetPerTokens { get; init; } = 50;
    public int MinSeparation { get; init; } = 100;
    public bool AllowBoundaryCross { get; init; } = false;
    public bool UseDomainStopwords { get; init; } = true;
    public bool DetectSection { get; init; } = true;
    public int AsrPrefixTokens { get; init; } = 8;
    public bool EmitWindows { get; init; } = true;
    public SectionRange? SectionOverride { get; init; }
    public bool TryResolveSectionFromLabels { get; init; } = true;
}

public sealed record TranscriptBuildOptions
{
    public string? AudioPath { get; init; }
    public string? ScriptPath { get; init; }
    public string? BookIndexPath { get; init; }
    public AnchorComputationOptions AnchorOptions { get; init; } = new();
}

public sealed record HydrationOptions
{
    public bool RecomputeDiffs { get; init; } = true;
}
