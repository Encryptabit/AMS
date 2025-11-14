using System.Net.Http.Json;
using Ams.Web.Dtos;

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
}
