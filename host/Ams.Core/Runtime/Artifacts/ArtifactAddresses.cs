using Ams.Core.Runtime.Book;

namespace Ams.Core.Runtime.Artifacts;

public sealed record ChapterArtifactAddress
{
    public ChapterArtifactAddress(string chapterRoot, string chapterId, string suffix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(suffix);

        if (chapterId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || chapterId.Contains(Path.DirectorySeparatorChar)
            || chapterId.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new ArgumentException(
                "Chapter id cannot contain path separators or invalid file name characters.",
                nameof(chapterId));
        }

        var normalizedSuffix = suffix.Trim().TrimStart('.');
        ArgumentOutOfRangeException.ThrowIfEqual(normalizedSuffix.Length, 0, nameof(suffix));

        if (normalizedSuffix.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || normalizedSuffix.Contains(Path.DirectorySeparatorChar)
            || normalizedSuffix.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new ArgumentException(
                "Artifact suffix cannot contain path separators or invalid file name characters.",
                nameof(suffix));
        }

        ChapterRoot = Path.GetFullPath(chapterRoot);
        ChapterId = chapterId;
        Suffix = normalizedSuffix;
        FileName = $"{chapterId}.{normalizedSuffix}";
    }

    private ChapterArtifactAddress(string chapterRoot, string chapterId, string suffix, string fileName)
        : this(chapterRoot, chapterId, suffix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || fileName.Contains(Path.DirectorySeparatorChar)
            || fileName.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new ArgumentException(
                "Artifact file name cannot contain path separators or invalid file name characters.",
                nameof(fileName));
        }

        FileName = fileName;
    }

    public string ChapterRoot { get; }
    public string ChapterId { get; }
    public string Suffix { get; }
    public string FileName { get; }

    public static ChapterArtifactAddress FromDescriptor(ChapterDescriptor descriptor, string suffix)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (string.IsNullOrWhiteSpace(descriptor.RootPath))
        {
            throw new InvalidOperationException(
                $"Chapter '{descriptor.ChapterId}' does not specify a root path.");
        }

        return new ChapterArtifactAddress(descriptor.RootPath, ResolveChapterStem(descriptor), suffix);
    }

    public static ChapterArtifactAddress FromFile(FileInfo file, string? chapterId = null)
    {
        ArgumentNullException.ThrowIfNull(file);

        var root = file.Directory?.FullName;
        if (string.IsNullOrWhiteSpace(root))
        {
            root = Directory.GetCurrentDirectory();
        }

        var fileName = file.Name;
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName, nameof(file));
        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || fileName.Contains(Path.DirectorySeparatorChar)
            || fileName.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new ArgumentException(
                "Artifact file name cannot contain path separators or invalid file name characters.",
                nameof(file));
        }

        var effectiveChapterId = string.IsNullOrWhiteSpace(chapterId)
            ? InferChapterId(fileName)
            : chapterId;
        var suffix = InferSuffix(fileName, effectiveChapterId);
        return new ChapterArtifactAddress(root, effectiveChapterId, suffix, fileName);
    }

    public static ChapterArtifactAddress FromPath(string path, string? chapterId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return FromFile(new FileInfo(path), chapterId);
    }

    public FileInfo ToFile() => new(Path.Combine(ChapterRoot, FileName));

    public string FullPath => ToFile().FullName;

    public override string ToString() => FullPath;

    private static string ResolveChapterStem(ChapterDescriptor descriptor)
    {
        if (!string.IsNullOrWhiteSpace(descriptor.ChapterId))
        {
            return descriptor.ChapterId;
        }

        var root = Path.GetFullPath(descriptor.RootPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var stem = Path.GetFileName(root);
        return string.IsNullOrWhiteSpace(stem)
            ? throw new InvalidOperationException("Chapter artifact stem could not be resolved.")
            : stem;
    }

    private static string InferChapterId(string fileName)
    {
        var stem = Path.GetFileNameWithoutExtension(fileName);
        var firstSegment = stem.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(firstSegment)
            ? throw new ArgumentException("Artifact file name must contain a chapter id.", nameof(fileName))
            : firstSegment;
    }

    private static string InferSuffix(string fileName, string chapterId)
    {
        var canonicalPrefix = $"{chapterId}.";
        if (fileName.StartsWith(canonicalPrefix, StringComparison.OrdinalIgnoreCase)
            && fileName.Length > canonicalPrefix.Length)
        {
            var suffix = fileName[canonicalPrefix.Length..].Trim().TrimStart('.');
            ArgumentOutOfRangeException.ThrowIfEqual(suffix.Length, 0, nameof(fileName));
            if (suffix.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                || suffix.Contains(Path.DirectorySeparatorChar)
                || suffix.Contains(Path.AltDirectorySeparatorChar))
            {
                throw new ArgumentException(
                    "Artifact suffix cannot contain path separators or invalid file name characters.",
                    nameof(fileName));
            }

            return suffix;
        }

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException("Artifact file name must contain a suffix.", nameof(fileName));
        }

        var extensionSuffix = extension.Trim().TrimStart('.');
        ArgumentOutOfRangeException.ThrowIfEqual(extensionSuffix.Length, 0, nameof(fileName));
        if (extensionSuffix.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || extensionSuffix.Contains(Path.DirectorySeparatorChar)
            || extensionSuffix.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new ArgumentException(
                "Artifact suffix cannot contain path separators or invalid file name characters.",
                nameof(fileName));
        }

        return extensionSuffix;
    }
}

public sealed record BookArtifactAddress
{
    public BookArtifactAddress(string bookRoot, string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bookRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || fileName.Contains(Path.DirectorySeparatorChar)
            || fileName.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new ArgumentException(
                "Artifact file name cannot contain path separators or invalid file name characters.",
                nameof(fileName));
        }

        BookRoot = Path.GetFullPath(bookRoot);
        FileName = fileName;
    }

    public string BookRoot { get; }
    public string FileName { get; }

    public FileInfo ToFile() => new(Path.Combine(BookRoot, FileName));

    public string FullPath => ToFile().FullName;

    public override string ToString() => FullPath;
}
