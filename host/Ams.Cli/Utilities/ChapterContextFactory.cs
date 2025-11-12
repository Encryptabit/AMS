using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Asr;
using Ams.Core.Processors.Alignment.Anchors;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Documents;

namespace Ams.Cli.Utilities;

public interface IChapterContextFactory
{
    ChapterContextHandle Create(
        FileInfo bookIndexFile,
        FileInfo? asrFile = null,
        FileInfo? transcriptFile = null,
        FileInfo? hydrateFile = null,
        FileInfo? audioFile = null,
        DirectoryInfo? chapterDirectory = null,
        string? chapterId = null);
}

public sealed class ChapterContextHandle : IDisposable
{
    private readonly IBookManager _bookManager;
    private string ChapterId => _bookManager.Current.Chapters.Current.Descriptor.ChapterId;
    private bool _disposed;

    internal ChapterContextHandle(IBookManager bookManager)
    {
        _bookManager = bookManager;
    }

    public BookContext Book => _bookManager.Current;
    public ChapterContext Chapter => _bookManager.Current.Chapters.Current;

    public void Save()
    {
        Chapter.Save();
        Book.Save();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _bookManager.Deallocate(ChapterId);
        _disposed = true;
    }
}

internal sealed class ChapterContextFactory : IChapterContextFactory
{
    private sealed record ManagerKey(string BookRoot);

    private readonly Dictionary<ManagerKey, BookManager> _managers = new();
    private readonly object _sync = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ChapterContextHandle Create(
        FileInfo bookIndexFile,
        FileInfo? asrFile = null,
        FileInfo? transcriptFile = null,
        FileInfo? hydrateFile = null,
        FileInfo? audioFile = null,
        DirectoryInfo? chapterDirectory = null,
        string? chapterId = null)
    {
        if (!bookIndexFile.Exists)
        {
            throw new FileNotFoundException("Book index not found", bookIndexFile.FullName);
        }

        var chapterStem = DetermineChapterStem(chapterId, audioFile, asrFile);
        var chapterRoot = ResolveChapterRoot(chapterDirectory, audioFile, asrFile, bookIndexFile.Directory, chapterStem);
        var audioPath = audioFile?.FullName ?? Path.Combine(chapterRoot, $"{chapterStem}.wav");
        var bookIndex = LoadJson<BookIndex>(bookIndexFile.FullName);

        var initialDescriptor = new ChapterDescriptor(
            chapterId: chapterStem,
            rootPath: chapterRoot,
            audioBuffers: new[]
            {
                new AudioBufferDescriptor("raw", audioPath)
            },
            aliases: BuildAliasSet(chapterStem, chapterRoot, bookIndex));

        var bookRoot = bookIndexFile.Directory?.FullName ?? Directory.GetCurrentDirectory();
        var bookDescriptor = new BookDescriptor(
            bookId: Path.GetFileName(bookRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
            rootPath: bookRoot,
            chapters: new[] { initialDescriptor });

        var manager = GetOrCreateManager(bookDescriptor);
        var book = manager.Current;
        var descriptor = EnsureChapterDescriptor(book, initialDescriptor, bookIndex);
        var chapter = book.Chapters.Load(descriptor.ChapterId);

        book.Documents.BookIndex = bookIndex;

        if (asrFile?.Exists == true)
        {
            chapter.Documents.Asr = LoadJson<AsrResponse>(asrFile.FullName);
        }

        if (transcriptFile?.Exists == true)
        {
            chapter.Documents.Transcript = LoadJson<TranscriptIndex>(transcriptFile.FullName);
        }

        if (hydrateFile?.Exists == true)
        {
            chapter.Documents.HydratedTranscript = LoadJson<HydratedTranscript>(hydrateFile.FullName);
        }

        return new ChapterContextHandle(manager);
    }

    private static ChapterDescriptor EnsureChapterDescriptor(
        BookContext book,
        ChapterDescriptor template,
        BookIndex bookIndex)
    {
        var chapters = book.Chapters;
        if (chapters.Contains(template.ChapterId))
        {
            return chapters.UpsertDescriptor(template);
        }

        var normalizedRequested = NormalizeChapterId(template.ChapterId);
        var aliasMatch = FindByAlias(chapters.Descriptors, normalizedRequested);
        if (aliasMatch is not null)
        {
            var merged = CloneWithAliases(aliasMatch, template);
            return chapters.UpsertDescriptor(merged);
        }

        var rootMatch = TryMatchByRootPath(chapters.Descriptors, template.RootPath);
        if (rootMatch is not null)
        {
            var merged = CloneWithAliases(rootMatch, template);
            return chapters.UpsertDescriptor(merged);
        }

        var slugMatch = TryMatchByRootSlug(chapters.Descriptors, normalizedRequested);
        if (slugMatch is not null)
        {
            var merged = CloneWithAliases(slugMatch, template);
            return chapters.UpsertDescriptor(merged);
        }

        return chapters.UpsertDescriptor(template);
    }

    private static ChapterDescriptor? FindByAlias(IReadOnlyList<ChapterDescriptor> descriptors, string normalizedAlias)
    {
        if (string.IsNullOrEmpty(normalizedAlias))
        {
            return null;
        }

        foreach (var descriptor in descriptors)
        {
            if (string.Equals(NormalizeChapterId(descriptor.ChapterId), normalizedAlias, StringComparison.OrdinalIgnoreCase))
            {
                return descriptor;
            }

            foreach (var alias in descriptor.Aliases)
            {
                if (string.Equals(NormalizeChapterId(alias), normalizedAlias, StringComparison.OrdinalIgnoreCase))
                {
                    return descriptor;
                }
            }
        }

        return null;
    }

    private static ChapterDescriptor? TryMatchByRootPath(IReadOnlyList<ChapterDescriptor> descriptors, string chapterRoot)
    {
        if (string.IsNullOrWhiteSpace(chapterRoot))
        {
            return null;
        }

        var normalizedRoot = NormalizePath(chapterRoot);
        foreach (var descriptor in descriptors)
        {
            if (string.IsNullOrWhiteSpace(descriptor.RootPath))
            {
                continue;
            }

            if (string.Equals(NormalizePath(descriptor.RootPath), normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                return descriptor;
            }
        }

        return null;
    }

    private static ChapterDescriptor? TryMatchByRootSlug(IReadOnlyList<ChapterDescriptor> descriptors, string normalizedRequested)
    {
        foreach (var descriptor in descriptors)
        {
            if (string.IsNullOrWhiteSpace(descriptor.RootPath))
            {
                continue;
            }

            var slug = Path.GetFileName(descriptor.RootPath);
            if (string.IsNullOrEmpty(slug))
            {
                continue;
            }

            if (string.Equals(NormalizeChapterId(slug), normalizedRequested, StringComparison.OrdinalIgnoreCase))
            {
                return descriptor;
            }
        }

        return null;
    }

    private static ChapterDescriptor CloneWithAliases(ChapterDescriptor existing, ChapterDescriptor incoming)
    {
        var aliases = MergeAliases(existing, incoming);
        var rootPath = string.IsNullOrWhiteSpace(incoming.RootPath) ? existing.RootPath : incoming.RootPath;
        var audioBuffers = existing.AudioBuffers.Count > 0 ? existing.AudioBuffers : incoming.AudioBuffers;
        var documents = existing.Documents ?? incoming.Documents;

        return new ChapterDescriptor(existing.ChapterId, rootPath, audioBuffers, documents, aliases);
    }

    private static IReadOnlyCollection<string> MergeAliases(ChapterDescriptor existing, ChapterDescriptor incoming)
    {
        var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddAlias(aliases, existing.ChapterId);
        AddAlias(aliases, incoming.ChapterId);

        foreach (var alias in existing.Aliases)
        {
            AddAlias(aliases, alias);
        }

        foreach (var alias in incoming.Aliases)
        {
            AddAlias(aliases, alias);
        }

        return aliases.ToArray();
    }

    private static IReadOnlyCollection<string> BuildAliasSet(string chapterId, string chapterRoot, BookIndex bookIndex)
    {
        var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddAlias(aliases, chapterId);

        if (!string.IsNullOrWhiteSpace(chapterRoot))
        {
            var rootName = Path.GetFileName(chapterRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            AddAlias(aliases, rootName);
        }

        var section = SectionLocator.ResolveSectionByTitle(bookIndex, chapterId);
        if (section is null && !string.IsNullOrWhiteSpace(chapterRoot))
        {
            var rootName = Path.GetFileName(chapterRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            section = SectionLocator.ResolveSectionByTitle(bookIndex, rootName);
        }

        if (section is not null && !string.IsNullOrWhiteSpace(section.Title))
        {
            AddAlias(aliases, section.Title);
        }

        return aliases.ToArray();
    }

    private static void AddAlias(ISet<string> aliases, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (aliases.Add(value))
        {
            var normalized = NormalizeChapterId(value);
            if (!string.IsNullOrEmpty(normalized))
            {
                aliases.Add(normalized);
            }
        }
    }

    private static string NormalizePath(string path)
    {
        var normalized = Path.GetFullPath(path);
        return normalized.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static string NormalizeChapterId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
        }

        return builder.ToString();
    }

    private BookManager GetOrCreateManager(BookDescriptor descriptor)
    {
        var key = new ManagerKey(descriptor.RootPath);
        lock (_sync)
        {
            if (!_managers.TryGetValue(key, out var manager))
            {
                manager = new BookManager(new[] { descriptor });
                _managers[key] = manager;
            }

            return manager;
        }
    }

    private static string DetermineChapterStem(string? supplied, FileInfo? audioFile, FileInfo? asrFile)
    {
        if (!string.IsNullOrWhiteSpace(supplied))
        {
            return supplied!;
        }

        var candidate = audioFile ?? asrFile;
        if (candidate is not null)
        {
            return Path.GetFileNameWithoutExtension(candidate.Name);
        }

        return "chapter";
    }

    private static string ResolveChapterRoot(
        DirectoryInfo? chapterDirectory,
        FileInfo? audioFile,
        FileInfo? asrFile,
        DirectoryInfo? bookIndexDirectory,
        string chapterStem)
    {
        if (chapterDirectory is not null)
        {
            Directory.CreateDirectory(chapterDirectory.FullName);
            return chapterDirectory.FullName;
        }

        var candidate = audioFile?.Directory ?? asrFile?.Directory ?? bookIndexDirectory;
        if (candidate is null)
        {
            var fallback = Path.Combine(Directory.GetCurrentDirectory(), chapterStem);
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        Directory.CreateDirectory(candidate.FullName);
        return candidate.FullName;
    }

    private static T LoadJson<T>(string path)
        => JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name} from {path}");
}
