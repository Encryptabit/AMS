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
                s.Timing?.StartSec,
                s.Timing?.EndSec,
                s.BookText ?? string.Empty,
                s.ScriptText ?? string.Empty,
                s.Metrics.Wer,
                s.Metrics.Cer))
            .ToArray();

        var paragraphs = (hydrate.Paragraphs ?? Array.Empty<Ams.Core.Artifacts.Hydrate.HydratedParagraph>())
            .Select(p => new ParagraphDto(
                p.Id,
                p.Status ?? string.Empty,
                p.SentenceIds ?? Array.Empty<int>(),
                p.Metrics.Wer))
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
        return Results.File(stream, "audio/wav");
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

// Records / DTOs
public sealed record AudioExportRequest(double? Start, double? End, string? Variant);
