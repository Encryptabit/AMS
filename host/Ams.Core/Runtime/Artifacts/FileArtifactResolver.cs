using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Artifacts.Alignment.Mfa;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Asr;
using Ams.Core.Processors.Alignment.Mfa;
using Ams.Core.Prosody;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Runtime.Artifacts;

public sealed class FileArtifactResolver : IArtifactResolver
{
    public static FileArtifactResolver Instance { get; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public BookIndex? LoadBookIndex(BookContext context)
    {
        var path = ResolveBookIndexPath(context);
        return File.Exists(path)
            ? JsonSerializer.Deserialize<BookIndex>(File.ReadAllText(path), JsonOptions)
            : null;
    }

    public void SaveBookIndex(BookContext context, BookIndex bookIndex)
    {
        ArgumentNullException.ThrowIfNull(bookIndex);
        var path = ResolveBookIndexPath(context);
        EnsureDirectory(path);
        File.WriteAllText(path, JsonSerializer.Serialize(bookIndex, JsonOptions));
    }

    public TranscriptIndex? LoadTranscript(ChapterContext context)
        => LoadJson<TranscriptIndex>(GetChapterArtifactPath(context, "align.tx.json"));

    public void SaveTranscript(ChapterContext context, TranscriptIndex transcript)
        => SaveJson(GetChapterArtifactPath(context, "align.tx.json"), transcript);

    public HydratedTranscript? LoadHydratedTranscript(ChapterContext context)
        => LoadJson<HydratedTranscript>(GetChapterArtifactPath(context, "align.hydrate.json"));

    public void SaveHydratedTranscript(ChapterContext context, HydratedTranscript hydrated)
        => SaveJson(GetChapterArtifactPath(context, "align.hydrate.json"), hydrated);

    public AnchorDocument? LoadAnchors(ChapterContext context)
        => LoadJson<AnchorDocument>(GetChapterArtifactPath(context, "align.anchors.json"));

    public void SaveAnchors(ChapterContext context, AnchorDocument document)
        => SaveJson(GetChapterArtifactPath(context, "align.anchors.json"), document);

    public AsrResponse? LoadAsr(ChapterContext context)
        => LoadJson<AsrResponse>(GetChapterArtifactPath(context, "asr.json"));

    public string? LoadAsrTranscriptText(ChapterContext context)
        => LoadText(GetChapterArtifactPath(context, "asr.corpus.txt"));

    public void SaveAsr(ChapterContext context, AsrResponse asr)
        => SaveJson(GetChapterArtifactPath(context, "asr.json"), asr);

    public void SaveAsrTranscriptText(ChapterContext context, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var path = GetChapterArtifactPath(context, "asr.corpus.txt");
        EnsureDirectory(path);
        File.WriteAllText(path, text);
    }

    public PausePolicy LoadPausePolicy(ChapterContext context)
    {
        var chapterPath = Path.Combine(GetChapterRoot(context.Descriptor), "pause-policy.json");
        if (File.Exists(chapterPath))
        {
            return PausePolicyStorage.Load(chapterPath);
        }

        var bookRoot = GetBookRoot(context.Book);
        var bookPath = Path.Combine(bookRoot, "pause-policy.json");
        if (File.Exists(bookPath))
        {
            return PausePolicyStorage.Load(bookPath);
        }

        return PausePolicyPresets.House();
    }

    public void SavePausePolicy(ChapterContext context, PausePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        var path = Path.Combine(GetChapterRoot(context.Descriptor), "pause-policy.json");
        PausePolicyStorage.Save(path, policy);
    }

    public PauseAdjustmentsDocument? LoadPauseAdjustments(ChapterContext context)
        => LoadPauseAdjustmentsInternal(GetChapterArtifactPath(context, "pause-adjustments.json"));

    public void SavePauseAdjustments(ChapterContext context, PauseAdjustmentsDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var path = GetChapterArtifactPath(context, "pause-adjustments.json");
        EnsureDirectory(path);
        document.Save(path);
    }

    public TextGridDocument? LoadTextGrid(ChapterContext context)
    {
        var path = GetTextGridPath(context);
        if (!File.Exists(path))
        {
            return null;
        }

        var intervals = TextGridParser.ParseWordIntervals(path).ToList();
        return new TextGridDocument(path, DateTime.UtcNow, intervals);
    }

    public void SaveTextGrid(ChapterContext context, TextGridDocument document)
    {
        _ = context;
        _ = document;
        // TextGrid documents are derived from the MFA output TextGrid file and do not need separate persistence.
    }

    private static PauseAdjustmentsDocument? LoadPauseAdjustmentsInternal(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return PauseAdjustmentsDocument.Load(path);
        }
        catch
        {
            return null;
        }
    }

    private static T? LoadJson<T>(string path)
    {
        if (!File.Exists(path))
        {
            return default;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    private static string? LoadText(string path)
        => File.Exists(path) ? File.ReadAllText(path) : null;

    private static void SaveJson<T>(string path, T payload)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(payload);
        EnsureDirectory(path);
        File.WriteAllText(path, JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static string ResolveBookIndexPath(BookContext context)
    {
        var root = GetBookRoot(context);
        return Path.Combine(root, "book-index.json");
    }

    private static string GetBookRoot(BookContext context)
    {
        var root = context.Descriptor.RootPath;
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new InvalidOperationException($"Book '{context.Descriptor.BookId}' does not specify a root path.");
        }

        return Path.GetFullPath(root);
    }

    private static string GetChapterRoot(ChapterDescriptor descriptor)
    {
        if (string.IsNullOrWhiteSpace(descriptor.RootPath))
        {
            throw new InvalidOperationException($"Chapter '{descriptor.ChapterId}' does not specify a root path.");
        }

        return Path.GetFullPath(descriptor.RootPath);
    }

    private static string GetChapterStem(ChapterDescriptor descriptor)
    {
        if (!string.IsNullOrWhiteSpace(descriptor.ChapterId))
        {
            return descriptor.ChapterId;
        }

        var root = GetChapterRoot(descriptor).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return Path.GetFileName(root);
    }

    private static string GetChapterArtifactPath(ChapterContext context, string suffix)
    {
        var directory = GetChapterRoot(context.Descriptor);
        var stem = GetChapterStem(context.Descriptor);
        return Path.Combine(directory, $"{stem}.{suffix}");
    }

    private static string GetTextGridPath(ChapterContext context)
    {
        var chapterRoot = GetChapterRoot(context.Descriptor);
        var stem = GetChapterStem(context.Descriptor);
        var alignmentDir = Path.Combine(chapterRoot, "alignment", "mfa");
        return Path.Combine(alignmentDir, $"{stem}.TextGrid");
    }

    private static void EnsureDirectory(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
