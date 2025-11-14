using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Ams.Web.Services;

public sealed class ChapterCatalogService
{
    private readonly WorkspaceState _workspaceState;
    private readonly ILogger<ChapterCatalogService> _logger;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new(StringComparer.OrdinalIgnoreCase);

    public ChapterCatalogService(WorkspaceState workspaceState, ILogger<ChapterCatalogService> logger)
    {
        _workspaceState = workspaceState ?? throw new ArgumentNullException(nameof(workspaceState));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _workspaceState.Changed += (_, _) => _cache.Clear();
    }

    public FileInfo BookIndexFile => new(_workspaceState.BookIndexPath);

    public async Task<IReadOnlyList<ChapterSummary>> GetChaptersAsync(CancellationToken cancellationToken)
    {
        var workspaceRoot = _workspaceState.WorkspaceRoot;
        if (!Directory.Exists(workspaceRoot))
        {
            _logger.LogWarning("Workspace root {Root} does not exist.", workspaceRoot);
            return Array.Empty<ChapterSummary>();
        }

        var results = new List<ChapterSummary>();
        foreach (var chapterDirectory in Directory.GetDirectories(workspaceRoot))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var summary = await BuildSummaryAsync(chapterDirectory, cancellationToken).ConfigureAwait(false);
            if (summary is not null)
            {
                results.Add(summary);
            }
        }

        results.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
        return results;
    }

    public Task<ChapterSummary?> GetChapterAsync(string chapterId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterId);
        var directory = Path.Combine(_workspaceState.WorkspaceRoot, chapterId);
        return Directory.Exists(directory)
            ? BuildSummaryAsync(directory, cancellationToken)
            : Task.FromResult<ChapterSummary?>(null);
    }

    private async Task<ChapterSummary?> BuildSummaryAsync(string chapterDirectory, CancellationToken cancellationToken)
    {
        var hydratePath = FindHydrateFile(chapterDirectory);
        if (hydratePath is null)
        {
            _logger.LogDebug("Skipping chapter at {Directory} because hydrate file was not found.", chapterDirectory);
            return null;
        }

        var hydrateInfo = new FileInfo(hydratePath);
        var cacheKey = Path.GetFileName(chapterDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var hydrateTimestamp = hydrateInfo.LastWriteTimeUtc;
        var audioPath = FindPreferredAudio(chapterDirectory);

        if (_cache.TryGetValue(cacheKey, out var entry) && entry.TimestampUtc == hydrateTimestamp)
        {
            if (audioPath is not null && audioPath != entry.Summary.AudioPath)
            {
                var updated = entry.Summary with { AudioPath = audioPath };
                _cache[cacheKey] = entry with { Summary = updated };
                return updated;
            }

            return entry.Summary;
        }

        var metadata = await HydrateMetadataExtractor.ReadAsync(hydratePath, cancellationToken).ConfigureAwait(false);
        var summary = new ChapterSummary(
            Id: cacheKey,
            DisplayName: cacheKey,
            RootPath: chapterDirectory,
            HydratePath: hydratePath,
            AudioPath: audioPath,
            SentenceCount: metadata.SentenceCount,
            DurationSeconds: metadata.DurationSeconds,
            HydrateUpdatedUtc: hydrateTimestamp,
            HydrateCreatedUtc: metadata.CreatedAtUtc);

        _cache[cacheKey] = new CacheEntry(summary, hydrateTimestamp);
        return summary;
    }

    private static string? FindHydrateFile(string directory)
        => Directory.EnumerateFiles(directory, "*.align.hydrate.json", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();

    private static string? FindPreferredAudio(string directory)
    {
        static string? Find(string dir, string pattern)
            => Directory.EnumerateFiles(dir, pattern, SearchOption.TopDirectoryOnly).FirstOrDefault();

        return Find(directory, "*.treated.wav")
               ?? Find(directory, "*.dsp.wav")
               ?? Find(directory, "*.wav");
    }

    private sealed record CacheEntry(ChapterSummary Summary, DateTime TimestampUtc);
}

public sealed record ChapterSummary(
    string Id,
    string DisplayName,
    string RootPath,
    string HydratePath,
    string? AudioPath,
    int SentenceCount,
    double DurationSeconds,
    DateTime HydrateUpdatedUtc,
    DateTime HydrateCreatedUtc);

internal static class HydrateMetadataExtractor
{
    public static async Task<HydrateMetadata> ReadAsync(string hydratePath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(hydratePath);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

        var root = document.RootElement;
        var createdAtUtc = root.TryGetProperty("createdAtUtc", out var createdProp) &&
                           createdProp.TryGetDateTime(out var created)
            ? DateTime.SpecifyKind(created, DateTimeKind.Utc)
            : DateTime.MinValue;

        if (!root.TryGetProperty("sentences", out var sentencesElement) ||
            sentencesElement.ValueKind != JsonValueKind.Array)
        {
            return new HydrateMetadata(0, 0, createdAtUtc);
        }

        var sentenceCount = 0;
        var maxEnd = 0d;

        foreach (var sentence in sentencesElement.EnumerateArray())
        {
            cancellationToken.ThrowIfCancellationRequested();
            sentenceCount++;
            if (sentence.TryGetProperty("timing", out var timingElement) &&
                timingElement.TryGetProperty("endSec", out var endProp) &&
                endProp.TryGetDouble(out var end))
            {
                maxEnd = Math.Max(maxEnd, end);
            }
        }

        return new HydrateMetadata(sentenceCount, maxEnd, createdAtUtc);
    }
}

internal sealed record HydrateMetadata(int SentenceCount, double DurationSeconds, DateTime CreatedAtUtc);
