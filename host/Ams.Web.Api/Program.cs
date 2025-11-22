using System.Text.Json.Serialization;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Workspace;
using Ams.Core.Runtime.Artifacts;
using Ams.Web.Api.Dtos.Validation;
using Ams.Web.Api.Mappers;
using Ams.Web.Api.Json;
using Ams.Web.Api.Services;
using Ams.Web.Api.Payloads;
using Ams.Core.Artifacts;
using Ams.Core.Processors;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Audio;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Clear();
    options.SerializerOptions.TypeInfoResolverChain.Add(ApiJsonSerializerContext.Default);
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddSingleton<WorkspaceState>();
builder.Services.AddSingleton<ValidationMapper>();
builder.Services.AddSingleton<ReviewedStateService>();

var app = builder.Build();

// Workspace endpoints
app.MapGet("/workspace", (WorkspaceState state) => Results.Ok(state.ToResponse()));

app.MapPost("/workspace", (WorkspaceState state, WorkspaceRequest request) =>
{
    state.Update(request);
    return Results.Ok(state.ToResponse());
});

// Simple chapters listing using Ams.Core (ChapterManager handles its own cache)
app.MapGet("/validation/books/{bookId}/chapters", (ValidationMapper mapper, WorkspaceState state, string bookId) =>
{
    try
    {
        var book = state.GetBook(bookId);

        var summaries = new List<ChapterSummaryResponse>();
        
        foreach (var descriptor in book.Chapters.Descriptors)
        {
            var chapter = book.Chapters.Load(descriptor.ChapterId);
            var hydrate = chapter.Documents.HydratedTranscript;
            var sentenceCount = hydrate?.Sentences?.Count ?? 0;
            var paragraphCount = hydrate?.Paragraphs?.Count ?? 0;

            summaries.Add(new ChapterSummaryResponse(
                descriptor.ChapterId,
                descriptor.RootPath,
                hydrate is not null,
                sentenceCount,
                paragraphCount));
        }

        var dto = summaries.Select(mapper.Map).ToArray();
        return Results.Json(dto, ApiJsonSerializerContext.Default.ValidationChapterSummaryDtoArray);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Overview
app.MapGet("/validation/books/{bookId}/overview", (WorkspaceState state, string bookId) =>
{
    var book = state.GetBook(bookId);
    var descriptors = book.Chapters.Descriptors;
    int totalSentences = 0, totalParagraphs = 0;

    foreach (var descriptor in descriptors)
    {
        var chapter = book.Chapters.Load(descriptor.ChapterId);
        var hydrate = chapter.Documents.HydratedTranscript;
        totalSentences += hydrate?.Sentences?.Count ?? 0;
        totalParagraphs += hydrate?.Paragraphs?.Count ?? 0;
    }

    var dto = new ValidationOverviewDto(bookId, descriptors.Count, totalSentences, totalParagraphs);
    return Results.Json(dto, ApiJsonSerializerContext.Default.ValidationOverviewDto);
});

// Chapter report
app.MapGet("/validation/books/{bookId}/report/{chapterId}", (WorkspaceState state, string bookId, string chapterId) =>
{
    try
    {
        var book = state.GetBook(bookId);
        if (!book.Chapters.Contains(chapterId)) return Results.NotFound();

        var chapter = book.Chapters.Load(chapterId);
        var hydrate = chapter.Documents.HydratedTranscript;
        if (hydrate is null) return Results.NotFound();

        var report = new ValidationReportDto(
            chapterId,
            hydrate.ScriptPath,
            hydrate.Sentences?.Count ?? 0,
            hydrate.Paragraphs?.Count ?? 0);

        return Results.Json(report, ApiJsonSerializerContext.Default.ValidationReportDto);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Reviewed state
app.MapGet("/validation/books/{bookId}/reviewed", (ReviewedStateService reviewed, string bookId) =>
{
    var data = reviewed.Get(bookId);
    return Results.Json(new ReviewedStatusResponse(data), ApiJsonSerializerContext.Default.ReviewedStatusResponse);
});

app.MapPost("/validation/books/{bookId}/reviewed/{chapterId}", (ReviewedStateService reviewed, string bookId, string chapterId, ReviewedStatusDto body) =>
{
    var updated = reviewed.Set(bookId, chapterId, body.Reviewed);
    return Results.Json(new ReviewedStatusResponse(updated), ApiJsonSerializerContext.Default.ReviewedStatusResponse);
});

app.MapPost("/validation/books/{bookId}/reset-reviews", (ReviewedStateService reviewed, string bookId) =>
{
    reviewed.Reset(bookId);
    return Results.Ok();
});

// Chapter detail for errors/playback
app.MapGet("/validation/books/{bookId}/chapters/{chapterId}", (WorkspaceState state, string bookId, string chapterId) =>
{
    try
    {
        var (book, chapter) = ResolveChapter(state, bookId, chapterId);
        var hydrate = chapter.Documents.HydratedTranscript;
        if (hydrate is null) return Results.NotFound();

        var sentences = (hydrate.Sentences ?? Array.Empty<Ams.Core.Artifacts.Hydrate.HydratedSentence>())
            .Select(s => new SentenceDto(
                s.Id,
                s.Status ?? string.Empty,
                s.Timing is null ? null : new TimingDto(s.Timing.StartSec, s.Timing.EndSec, s.Timing.Duration),
                new RangeDto(s.BookRange.Start, s.BookRange.End),
                s.ScriptRange is null ? null : new RangeDto(s.ScriptRange.Start ?? 0, s.ScriptRange.End ?? 0),
                s.BookText ?? string.Empty,
                s.ScriptText ?? string.Empty,
                new MetricsDto(s.Metrics.Wer, s.Metrics.Cer, s.Metrics.SpanWer, s.Metrics.MissingRuns, s.Metrics.ExtraRuns),
                s.Diff is null
                    ? null
                    : new DiffDto(
                        s.Diff.Ops?.Select(op => new DiffOpDto(op.Operation ?? string.Empty, op.Tokens ?? Array.Empty<string>())).ToArray() ?? Array.Empty<DiffOpDto>(),
                        s.Diff.Stats is null ? new DiffStatsDto(0, 0, 0, 0, 0) :
                            new DiffStatsDto(s.Diff.Stats.ReferenceTokens, s.Diff.Stats.HypothesisTokens, s.Diff.Stats.Matches, s.Diff.Stats.Insertions, s.Diff.Stats.Deletions))))
            .ToArray();

        var paragraphs = (hydrate.Paragraphs ?? Array.Empty<Ams.Core.Artifacts.Hydrate.HydratedParagraph>())
            .Select(p => new ParagraphDto(
                p.Id,
                p.Status ?? string.Empty,
                new RangeDto(p.BookRange.Start, p.BookRange.End),
                p.SentenceIds ?? Array.Empty<int>(),
                p.BookText ?? string.Empty,
                new ParagraphMetricsDto(p.Metrics.Wer, p.Metrics.Cer, p.Metrics.Coverage),
                p.Diff is null
                    ? null
                    : new DiffDto(
                        p.Diff.Ops?.Select(op => new DiffOpDto(op.Operation ?? string.Empty, op.Tokens ?? Array.Empty<string>())).ToArray() ?? Array.Empty<DiffOpDto>(),
                        p.Diff.Stats is null ? new DiffStatsDto(0, 0, 0, 0, 0) :
                            new DiffStatsDto(p.Diff.Stats.ReferenceTokens, p.Diff.Stats.HypothesisTokens, p.Diff.Stats.Matches, p.Diff.Stats.Insertions, p.Diff.Stats.Deletions))))
            .ToArray();

        var dto = new ChapterDetailDto(
            chapterId,
            true,
            sentences.Length,
            paragraphs.Length,
            sentences,
            paragraphs,
            new AudioAvailabilityDto(
                HasBuffer(chapter, "raw"),
                HasBuffer(chapter, "treated"),
                HasBuffer(chapter, "filtered")));

        return Results.Json(dto, ApiJsonSerializerContext.Default.ChapterDetailDto);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Audio streaming
app.MapGet("/audio/books/{bookId}/chapters/{chapterId}", (WorkspaceState state, string bookId, string chapterId, string? variant, double? start, double? end) =>
{
    try
    {
        var (_, chapter) = ResolveChapter(state, bookId, chapterId);
        var buffer = TryLoadBuffer(chapter, variant ?? "raw");
        if (buffer is null) return Results.NotFound();

        var slice = Slice(buffer, start, end);
        var stream = slice.ToWavStream();
        stream.Position = 0;
        return Results.File(stream, "audio/wav", enableRangeProcessing: true);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Audio export (CRX-friendly slice)
app.MapPost("/audio/books/{bookId}/chapters/{chapterId}/export", async (WorkspaceState state, string bookId, string chapterId, AudioExportRequest body) =>
{
    try
    {
        var (book, chapter) = ResolveChapter(state, bookId, chapterId);
        var buffer = TryLoadBuffer(chapter, body.Variant ?? "treated") ?? TryLoadBuffer(chapter, "raw");
        if (buffer is null) return Results.NotFound();

        var slice = Slice(buffer, body.Start, body.End);
        var crxDir = GetCrxDir(book, state);
        crxDir.Create();
        var errorNum = NextErrorNumber(crxDir);
        var exportFile = new FileInfo(Path.Combine(crxDir.FullName, $"{errorNum:000}.wav"));
        await using (var wav = slice.ToWavStream())
        await using (var fs = exportFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            wav.Position = 0;
            await wav.CopyToAsync(fs);
        }

        var response = new AudioExportResponse(errorNum, exportFile.Name, Path.GetRelativePath(book.Descriptor.RootPath, exportFile.FullName));
        return Results.Json(response, ApiJsonSerializerContext.Default.AudioExportResponse);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// CRX add (delegates to audio export for now)
app.MapPost("/validation/books/{bookId}/crx/{chapterId}", async (WorkspaceState state, string bookId, string chapterId, AudioExportRequest body) =>
{
    try
    {
        var (book, chapter) = ResolveChapter(state, bookId, chapterId);
        var buffer = TryLoadBuffer(chapter, body.Variant ?? "treated") ?? TryLoadBuffer(chapter, "raw");
        if (buffer is null) return Results.NotFound();

        var slice = Slice(buffer, body.Start, body.End);
        var crxDir = GetCrxDir(book, state);
        crxDir.Create();
        var errorNum = NextErrorNumber(crxDir);
        var exportFile = new FileInfo(Path.Combine(crxDir.FullName, $"{errorNum:000}.wav"));
        await using (var wav = slice.ToWavStream())
        await using (var fs = exportFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            wav.Position = 0;
            await wav.CopyToAsync(fs);
        }

        var response = new AudioExportResponse(errorNum, exportFile.Name, Path.GetRelativePath(book.Descriptor.RootPath, exportFile.FullName));
        return Results.Json(response, ApiJsonSerializerContext.Default.AudioExportResponse);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();

// Helpers
static (BookContext Book, ChapterContext Chapter) ResolveChapter(WorkspaceState state, string bookId, string chapterId)
{
    var book = state.GetBook(bookId);
    if (!book.Chapters.Contains(chapterId))
    {
        throw new KeyNotFoundException($"Chapter '{chapterId}' not found in book '{bookId}'.");
    }

    var chapter = book.Chapters.Load(chapterId);
    return (book, chapter);
}

static bool HasBuffer(ChapterContext chapter, string bufferId)
{
    return chapter.Descriptor.AudioBuffers.Any(b => string.Equals(b.BufferId, bufferId, StringComparison.OrdinalIgnoreCase));
}

static AudioBuffer? TryLoadBuffer(ChapterContext chapter, string variant)
{
    try
    {
        var ctx = ResolveAudioContext(chapter, variant);
        if (ctx is null) return null;

        var buffer = ctx.Buffer;
        if (buffer is not null) return buffer;

        var desc = ctx.Descriptor;
        if (TryResolveExistingPath(desc, chapter, out var path))
        {
            return AudioProcessor.Decode(path, new AudioDecodeOptions
            {
                TargetSampleRate = desc.SampleRate,
                TargetChannels = desc.Channels
            });
        }
    }
    catch
    {
        // ignore
    }
    return null;
}

static AudioBuffer Slice(AudioBuffer buffer, double? startSec, double? endSec)
{
    if (startSec is null || endSec is null || endSec <= startSec)
    {
        return buffer;
    }

    var startSample = (int)Math.Clamp(startSec.Value * buffer.SampleRate, 0, buffer.Length);
    var endSample = (int)Math.Clamp(endSec.Value * buffer.SampleRate, startSample, buffer.Length);
    var length = endSample - startSample;
    if (length <= 0) return buffer;

    var slice = new AudioBuffer(buffer.Channels, buffer.SampleRate, length, buffer.Metadata);
    for (var ch = 0; ch < buffer.Channels; ch++)
    {
        Array.Copy(buffer.Planar[ch], startSample, slice.Planar[ch], 0, length);
    }
    return slice;
}

static DirectoryInfo GetCrxDir(BookContext book, WorkspaceState state)
{
    var root = book.Descriptor.RootPath;
    var dir = new DirectoryInfo(Path.Combine(root, state.CrxDirectoryName));
    return dir;
}

static int NextErrorNumber(DirectoryInfo crxDir)
{
    var max = 0;
    foreach (var file in crxDir.EnumerateFiles("*.wav"))
    {
        if (int.TryParse(Path.GetFileNameWithoutExtension(file.Name), out var n))
        {
            max = Math.Max(max, n);
        }
    }
    return max + 1;
}

static AudioBufferContext? ResolveAudioContext(ChapterContext chapter, string variant)
{
    var bufferId = variant.ToLowerInvariant() switch
    {
        "treated" => "treated",
        "filtered" => "filtered",
        "raw" => "raw",
        _ => variant
    };

    try
    {
        // If specific buffer not found, fall back to first registered
        AudioBufferContext ctx;
        try
        {
            ctx = chapter.Audio.Load(bufferId);
        }
        catch
        {
            if (chapter.Descriptor.AudioBuffers.Count == 0) return null;
            ctx = chapter.Audio.Load(chapter.Descriptor.AudioBuffers[0].BufferId);
        }

        return ctx;
    }
    catch
    {
        return null;
    }
}

static bool TryResolveExistingPath(AudioBufferDescriptor desc, ChapterContext chapter, out string path)
{
    path = desc.Path;
    if (File.Exists(path))
    {
        return true;
    }

    // Fallback: look in book root for raw filename
    var bookRoot = chapter.Book.Descriptor.RootPath;
    var fallback = Path.Combine(bookRoot, Path.GetFileName(desc.Path));
    if (File.Exists(fallback))
    {
        path = fallback;
        return true;
    }

    return false;
}

// Records / DTOs
public sealed record AudioExportRequest(double? Start, double? End, string? Variant);
