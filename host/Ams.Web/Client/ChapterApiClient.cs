using System.Net.Http.Json;
using Ams.Web.Dtos;
using Ams.Web.Requests;

namespace Ams.Web.Client;

public sealed class ChapterApiClient
{
    private readonly HttpClient _httpClient;

    public ChapterApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ChapterListItemDto>> GetChaptersAsync(CancellationToken cancellationToken = default)
    {
        var chapters = await _httpClient.GetFromJsonAsync<IReadOnlyList<ChapterListItemDto>>(
            "api/chapters",
            cancellationToken).ConfigureAwait(false);

        return chapters ?? Array.Empty<ChapterListItemDto>();
    }

    public async Task<ChapterDetailDto?> GetChapterAsync(string chapterId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterId);
        var encodedId = Uri.EscapeDataString(chapterId);
        return await _httpClient.GetFromJsonAsync<ChapterDetailDto>(
            $"api/chapters/{encodedId}",
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<SentenceExportResponse> ExportSentenceAsync(
        string chapterId,
        int sentenceId,
        ExportSentenceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterId);
        var encodedChapter = Uri.EscapeDataString(chapterId);
        var response = await _httpClient.PostAsJsonAsync(
            $"api/chapters/{encodedChapter}/sentences/{sentenceId}/export",
            request ?? new ExportSentenceRequest(),
            cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException(
                $"Export failed ({(int)response.StatusCode} {response.ReasonPhrase}): {body}");
        }

        var payload = await response.Content.ReadFromJsonAsync<SentenceExportResponse>(
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return payload ?? throw new InvalidOperationException("Export response was empty.");
    }
}
