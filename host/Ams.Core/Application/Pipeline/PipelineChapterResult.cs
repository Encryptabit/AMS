namespace Ams.Core.Application.Pipeline;

public sealed record PipelineChapterResult(
    string ChapterId,
    bool BookIndexBuilt,
    bool AsrRan,
    bool AnchorsRan,
    bool TranscriptRan,
    bool HydrateRan,
    bool MfaRan,
    FileInfo BookIndexFile,
    FileInfo AsrFile,
    FileInfo AnchorFile,
    FileInfo TranscriptFile,
    FileInfo HydrateFile,
    FileInfo TextGridFile,
    FileInfo TreatedAudioFile);
