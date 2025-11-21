namespace Ams.Core.Artifacts.Alignment;

public sealed record class MfaChapterContext
{
    public required string CorpusDirectory { get; init; }
    public required string OutputDirectory { get; init; }
    public string? WorkingDirectory { get; init; }
    public string? DictionaryModel { get; init; }
    public string? AcousticModel { get; init; }
    public string? G2pModel { get; init; }
    public string? OovListPath { get; init; }
    public string? G2pOutputPath { get; init; }
    public string? CustomDictionaryPath { get; init; }
    public int? Beam { get; init; }
    public int? RetryBeam { get; init; }
    public bool? SingleSpeaker { get; init; }
    public bool? CleanOutput { get; init; }
}
