using System.Text.Json.Serialization;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Workspace;
using Ams.Core.Runtime.Artifacts;
using Ams.Web.Api.Dtos;
using Ams.Web.Api.Dtos.Validation;
using Ams.Web.Api.Mappers;
using Ams.Web.Api.Json;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Clear();
    options.SerializerOptions.TypeInfoResolverChain.Add(ApiJsonSerializerContext.Default);
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddSingleton<WorkspaceState>();
builder.Services.AddSingleton<ValidationMapper>();

var app = builder.Build();

// Workspace endpoints
app.MapGet("/workspace", (WorkspaceState state) => Results.Ok(state.ToResponse()));

app.MapPost("/workspace", (WorkspaceState state, WorkspaceRequest request) =>
{
    state.Update(request);
    return Results.Ok(state.ToResponse());
});

// Simple chapters listing using Ams.Core
app.MapGet("/validation/books/{bookId}/chapters", (WorkspaceState state, ValidationMapper mapper, string bookId) =>
{
    try
    {
        var book = ResolveBook(state, bookId);
        var summaries = new List<ChapterSummaryResponse>();

        foreach (var descriptor in book.Chapters.Descriptors)
        {
            var chapter = book.Chapters.Load(descriptor.ChapterId);
            var hydrate = chapter.Documents.HydratedTranscript;
            // Avoid reflection serialization of hydrate by extracting primitive counts only.
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

// Stub: overview
app.MapGet("/validation/books/{bookId}/overview", (string bookId) => Results.StatusCode(StatusCodes.Status501NotImplemented));

// Stub: chapter report
app.MapGet("/validation/books/{bookId}/report/{chapterId}", (string bookId, string chapterId) => Results.StatusCode(StatusCodes.Status501NotImplemented));

// Stub: reviewed state
app.MapGet("/validation/books/{bookId}/reviewed", (string bookId) => Results.StatusCode(StatusCodes.Status501NotImplemented));
app.MapPost("/validation/books/{bookId}/reviewed/{chapterId}", (string bookId, string chapterId) => Results.StatusCode(StatusCodes.Status501NotImplemented));
app.MapPost("/validation/books/{bookId}/reset-reviews", (string bookId) => Results.StatusCode(StatusCodes.Status501NotImplemented));

// Stub: audio
app.MapGet("/audio/books/{bookId}/chapters/{chapterId}", (string bookId, string chapterId) => Results.StatusCode(StatusCodes.Status501NotImplemented));
app.MapPost("/audio/books/{bookId}/chapters/{chapterId}/export", (string bookId, string chapterId) => Results.StatusCode(StatusCodes.Status501NotImplemented));

// Stub: CRX
app.MapPost("/validation/books/{bookId}/crx/{chapterId}", (string bookId, string chapterId) => Results.StatusCode(StatusCodes.Status501NotImplemented));

app.Run();

// Helpers
static BookContext ResolveBook(WorkspaceState state, string bookId)
{
    if (string.IsNullOrWhiteSpace(state.BookRoot))
    {
        throw new InvalidOperationException("WorkspaceRoot is not configured.");
    }

    var descriptors = WorkspaceChapterDiscovery.Discover(state.BookRoot);
    var bookDescriptor = new BookDescriptor(bookId, state.BookRoot, descriptors);
    var manager = new BookManager(new[] { bookDescriptor }, FileArtifactResolver.Instance);
    return manager.Current;
}

// Records / DTOs
public sealed record WorkspaceRequest(
    string? WorkspaceRoot,
    string? BookIndexPath,
    string? CrxTemplatePath,
    string? CrxDirectoryName,
    string? DefaultErrorType);

public sealed record WorkspaceResponse(
    string? WorkspaceRoot,
    string? BookIndexPath,
    string? CrxTemplatePath,
    string CrxDirectoryName,
    string DefaultErrorType);

public sealed record ChapterSummaryResponse(
    string Id,
    string Path,
    bool HasHydrate,
    int SentenceCount,
    int ParagraphCount);
