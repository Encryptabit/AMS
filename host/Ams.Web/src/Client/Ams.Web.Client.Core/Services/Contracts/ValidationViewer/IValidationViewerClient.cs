using Ams.Web.Shared.ValidationViewer;

namespace Ams.Web.Client.Core.Services.Contracts.ValidationViewer;

public interface IValidationViewerClient
{
    Task<IReadOnlyList<ValidationChapterSummary>> GetChaptersAsync(string bookId, CancellationToken ct = default);
    Task<ValidationOverviewResponse> GetOverviewAsync(string bookId, CancellationToken ct = default);
    Task<ValidationReportResponse> GetReportAsync(string bookId, string chapterId, CancellationToken ct = default);
}

