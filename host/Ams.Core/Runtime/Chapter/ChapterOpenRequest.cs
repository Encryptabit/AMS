using Ams.Core.Runtime.Artifacts;

namespace Ams.Core.Runtime.Chapter;

public sealed record ChapterOpenRequest
{
    public ChapterOpenRequest(
        FileInfo bookIndexFile,
        string chapterId,
        DirectoryInfo chapterDirectory,
        FileInfo? asrFile = null,
        FileInfo? transcriptFile = null,
        FileInfo? hydrateFile = null,
        FileInfo? audioFile = null,
        bool reloadBookIndex = false)
    {
        ArgumentNullException.ThrowIfNull(bookIndexFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterId);
        ArgumentNullException.ThrowIfNull(chapterDirectory);

        if (!bookIndexFile.Exists)
        {
            throw new FileNotFoundException("Book index not found", bookIndexFile.FullName);
        }

        var normalizedChapterDirectory = System.IO.Path.GetFullPath(chapterDirectory.FullName);
        Directory.CreateDirectory(normalizedChapterDirectory);

        BookIndexFile = bookIndexFile;
        ChapterId = chapterId;
        ChapterDirectory = new DirectoryInfo(normalizedChapterDirectory);
        AsrFile = asrFile;
        TranscriptFile = transcriptFile;
        HydrateFile = hydrateFile;
        AudioFile = audioFile;
        ReloadBookIndex = reloadBookIndex;
    }

    public FileInfo BookIndexFile { get; }
    public string ChapterId { get; }
    public DirectoryInfo ChapterDirectory { get; }
    public FileInfo? AsrFile { get; }
    public FileInfo? TranscriptFile { get; }
    public FileInfo? HydrateFile { get; }
    public FileInfo? AudioFile { get; }
    public bool ReloadBookIndex { get; }

    /// <summary>
    /// Builds a request from workspace/app state that has already crossed the user-input boundary.
    /// Failures here are contract violations, stale workspace state, or corrupted trusted state.
    /// </summary>
    public static ChapterOpenRequest FromTrusted(
        FileInfo bookIndexFile,
        FileInfo? asrFile = null,
        FileInfo? transcriptFile = null,
        FileInfo? hydrateFile = null,
        FileInfo? audioFile = null,
        DirectoryInfo? chapterDirectory = null,
        string? chapterId = null,
        bool reloadBookIndex = false)
    {
        ArgumentNullException.ThrowIfNull(bookIndexFile);
        if (!bookIndexFile.Exists)
        {
            throw new FileNotFoundException("Book index not found", bookIndexFile.FullName);
        }

        var resolvedChapterId = ResolveChapterId(chapterId, audioFile, asrFile);
        var resolvedChapterRoot = ResolveChapterRoot(
            chapterDirectory,
            audioFile,
            asrFile,
            bookIndexFile.Directory,
            resolvedChapterId);

        return new ChapterOpenRequest(
            bookIndexFile,
            resolvedChapterId,
            new DirectoryInfo(resolvedChapterRoot),
            asrFile,
            transcriptFile,
            hydrateFile,
            audioFile,
            reloadBookIndex);
    }

    public ChapterArtifactAddress RawAudioAddress()
        => AudioFile is null
            ? GetChapterArtifactAddress("wav")
            : ChapterArtifactAddress.FromFile(AudioFile, ChapterId);

    public ChapterArtifactAddress GetChapterArtifactAddress(string suffix)
        => new(ChapterDirectory.FullName, ChapterId, suffix);

    private static string ResolveChapterId(string? supplied, FileInfo? audioFile, FileInfo? asrFile)
    {
        if (!string.IsNullOrWhiteSpace(supplied))
        {
            return supplied;
        }

        var candidate = audioFile ?? asrFile;
        if (candidate is not null)
        {
            var chopped = candidate.Name.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var firstSegment = chopped.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstSegment))
            {
                return firstSegment;
            }
        }

        throw new ArgumentException("Chapter identifier must be provided.");
    }

    private static string ResolveChapterRoot(
        DirectoryInfo? chapterDirectory,
        FileInfo? audioFile,
        FileInfo? asrFile,
        DirectoryInfo? bookIndexDirectory,
        string chapterId)
    {
        if (chapterDirectory is not null)
        {
            Directory.CreateDirectory(chapterDirectory.FullName);
            return chapterDirectory.FullName;
        }

        var candidate = audioFile?.Directory ?? asrFile?.Directory ?? bookIndexDirectory;
        if (candidate is null)
        {
            var fallback = System.IO.Path.Combine(Directory.GetCurrentDirectory(), chapterId);
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        Directory.CreateDirectory(candidate.FullName);
        return candidate.FullName;
    }
}
