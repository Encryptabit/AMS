namespace Ams.Core.Alignment.Mfa;

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
}
