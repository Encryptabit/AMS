using System.Net.Http.Json;
using Ams.Web.Client.Core.Services.Contracts.ValidationViewer;
using Ams.Web.Shared.ValidationViewer;

namespace Ams.Web.Client.Core.Services.Http;

internal sealed class ValidationViewerClient : IValidationViewerClient
{
    private readonly HttpClient _http;

    public ValidationViewerClient(HttpClient http)
    {
        _http = http;
    }

    public Task<IReadOnlyList<ValidationChapterSummary>> GetChaptersAsync(string bookId, CancellationToken ct = default)
        => _http.GetFromJsonAsync<IReadOnlyList<ValidationChapterSummary>>($"api/validation/books/{bookId}/chapters", ct)!;

    public Task<ValidationOverviewResponse> GetOverviewAsync(string bookId, CancellationToken ct = default)
        => _http.GetFromJsonAsync<ValidationOverviewResponse>($"api/validation/books/{bookId}/overview", ct)!;

    public Task<ValidationReportResponse> GetReportAsync(string bookId, string chapterId, CancellationToken ct = default)
        => _http.GetFromJsonAsync<ValidationReportResponse>($"api/validation/books/{bookId}/report/{chapterId}", ct)!;
}

