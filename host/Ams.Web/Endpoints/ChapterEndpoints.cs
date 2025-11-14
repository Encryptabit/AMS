using Ams.Web.Dtos;
using Ams.Web.Mappers;
using Ams.Web.Requests;
using Ams.Web.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ams.Web.Endpoints;

public static class ChapterEndpoints
{
    public static IEndpointRouteBuilder MapChapterApi(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/chapters");

        group.MapGet("/", async Task<Results<Ok<IReadOnlyList<ChapterListItemDto>>, ProblemHttpResult>> (
            ChapterCatalogService catalog,
            ReviewedStatusStore reviewedStore,
            CancellationToken cancellationToken) =>
        {
            var chapters = await catalog.GetChaptersAsync(cancellationToken).ConfigureAwait(false);
            var payload = new List<ChapterListItemDto>(chapters.Count);
            foreach (var chapter in chapters)
            {
                var reviewed = await reviewedStore.GetAsync(chapter.Id, cancellationToken).ConfigureAwait(false);
                payload.Add(ChapterMapper.ToListItem(chapter, reviewed));
            }

            return TypedResults.Ok<IReadOnlyList<ChapterListItemDto>>(payload);
        });

        group.MapGet("/{chapterId}", async Task<Results<Ok<ChapterDetailDto>, NotFound>> (
            string chapterId,
            ChapterCatalogService catalog,
            ChapterDataService dataService,
            ReviewedStatusStore reviewedStore,
            CancellationToken cancellationToken) =>
        {
            var summary = await catalog.GetChapterAsync(chapterId, cancellationToken).ConfigureAwait(false);
            if (summary is null)
            {
                return TypedResults.NotFound();
            }

            var transcript = await dataService.LoadHydratedTranscriptAsync(summary, cancellationToken).ConfigureAwait(false);
            var sentences = transcript.Sentences.Select(ChapterMapper.ToSentenceDto).ToArray();
            var reviewed = await reviewedStore.GetAsync(summary.Id, cancellationToken).ConfigureAwait(false);
            var dto = ChapterMapper.ToDetail(summary, reviewed, sentences);

            return TypedResults.Ok(dto);
        });

        group.MapGet("/{chapterId}/audio", async Task<Results<PhysicalFileHttpResult, NotFound>> (
            string chapterId,
            ChapterCatalogService catalog,
            ChapterDataService dataService,
            CancellationToken cancellationToken) =>
        {
            var summary = await catalog.GetChapterAsync(chapterId, cancellationToken).ConfigureAwait(false);
            if (summary is null)
            {
                return TypedResults.NotFound();
            }

            var audioFile = await dataService.ResolveAudioFileAsync(summary, cancellationToken).ConfigureAwait(false);
            return TypedResults.PhysicalFile(audioFile.FullName, "audio/wav", enableRangeProcessing: true);
        });

        group.MapPost("/{chapterId}/reviewed", async Task<Results<Ok<ChapterListItemDto>, NotFound>> (
            string chapterId,
            SetReviewedRequest request,
            ChapterCatalogService catalog,
            ReviewedStatusStore reviewedStore,
            CancellationToken cancellationToken) =>
        {
            request ??= new SetReviewedRequest();
            var summary = await catalog.GetChapterAsync(chapterId, cancellationToken).ConfigureAwait(false);
            if (summary is null)
            {
                return TypedResults.NotFound();
            }

            await reviewedStore.SetAsync(chapterId, request?.Reviewed ?? false, cancellationToken).ConfigureAwait(false);
            var updated = ChapterMapper.ToListItem(summary, request?.Reviewed ?? false);
            return TypedResults.Ok(updated);
        });

        group.MapPost("/{chapterId}/sentences/{sentenceId:int}/export", async Task<Results<Ok<SentenceExportResponse>, NotFound, ProblemHttpResult>> (
            string chapterId,
            int sentenceId,
            ExportSentenceRequest request,
            ChapterCatalogService catalog,
            ChapterDataService dataService,
            CrxExportService exportService,
            CancellationToken cancellationToken) =>
        {
            request ??= new ExportSentenceRequest();
            var summary = await catalog.GetChapterAsync(chapterId, cancellationToken).ConfigureAwait(false);
            if (summary is null)
            {
                return TypedResults.NotFound();
            }

            var transcript = await dataService.LoadHydratedTranscriptAsync(summary, cancellationToken).ConfigureAwait(false);
            var sentence = transcript.Sentences.FirstOrDefault(s => s.Id == sentenceId);
            if (sentence is null)
            {
                return TypedResults.NotFound();
            }

            try
            {
                var result = await exportService.ExportAsync(summary, sentence, request, cancellationToken)
                    .ConfigureAwait(false);

                var response = new SentenceExportResponse(
                    summary.Id,
                    sentence.Id,
                    result.SegmentPath,
                    result.WorkbookPath,
                    result.RowNumber,
                    result.ErrorType,
                    result.StartSeconds,
                    result.EndSeconds);

                return TypedResults.Ok(response);
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(ex.Message);
            }
        });

        return builder;
    }
}
