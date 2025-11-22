using System.Net.Http.Json;
using Ams.Web.Shared.Validation;
using Ams.Web.Shared.Workspace;

namespace Ams.Web.Client.Services;

public sealed class ValidationApiClient
{
    private readonly HttpClient _http;

    public ValidationApiClient(HttpClient http)
    {
        _http = http;
    }

    public Task<WorkspaceResponse?> GetWorkspaceAsync(CancellationToken cancellationToken = default)
        => _http.GetFromJsonAsync<WorkspaceResponse>("/workspace", cancellationToken);

    public Task<HttpResponseMessage> SetWorkspaceAsync(WorkspaceRequest request,
        CancellationToken cancellationToken = default)
        => _http.PostAsJsonAsync("/workspace", request, cancellationToken);

    public IAsyncEnumerable<ValidationChapterSummaryDto?> StreamChaptersAsync(string bookId,
        CancellationToken cancellationToken = default)
        => _http.GetFromJsonAsAsyncEnumerable<ValidationChapterSummaryDto>(
            $"/validation/books/{bookId}/chapters", cancellationToken);

    public Task<ValidationOverviewDto?> GetOverviewAsync(string bookId, CancellationToken cancellationToken = default)
        => _http.GetFromJsonAsync<ValidationOverviewDto>($"/validation/books/{bookId}/overview", cancellationToken);

    public Task<ValidationReportDto?> GetReportAsync(string bookId, string chapterId,
        CancellationToken cancellationToken = default)
        => _http.GetFromJsonAsync<ValidationReportDto>($"/validation/books/{bookId}/report/{chapterId}",
            cancellationToken);

    public Task<ChapterDetailDto?> GetChapterDetailAsync(string bookId, string chapterId,
        CancellationToken cancellationToken = default)
        => _http.GetFromJsonAsync<ChapterDetailDto>($"/validation/books/{bookId}/chapters/{chapterId}",
            cancellationToken);

    public Task<ReviewedStatusResponse?> GetReviewedAsync(string bookId, CancellationToken cancellationToken = default)
        => _http.GetFromJsonAsync<ReviewedStatusResponse>($"/validation/books/{bookId}/reviewed", cancellationToken);

    public async Task SetReviewedAsync(string bookId, string chapterId, bool reviewed,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync($"/validation/books/{bookId}/reviewed/{chapterId}",
            new ReviewedStatusDto(reviewed, null), cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ResetReviewsAsync(string bookId, CancellationToken cancellationToken = default)
    {
        var response =
            await _http.PostAsJsonAsync($"/validation/books/{bookId}/reset-reviews", new { }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
