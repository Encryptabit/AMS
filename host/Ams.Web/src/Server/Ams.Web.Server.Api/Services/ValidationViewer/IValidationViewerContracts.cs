using Ams.Core.Artifacts;
using System.Text.Json;

namespace Ams.Web.Server.Api.Services.ValidationViewer;

public interface IValidationViewerService
{
    IReadOnlyList<Ams.Web.Shared.ValidationViewer.ValidationChapterSummary> GetChapters(string bookId);
    Ams.Web.Shared.ValidationViewer.ValidationOverviewResponse GetOverview(string bookId);
    Ams.Web.Shared.ValidationViewer.ValidationReportResponse? GetReport(string bookId, string chapterId);
}

public interface IAudioStreamService
{
    AudioBuffer? LoadBuffer(string bookId, string chapterId, string variant);
    AudioBuffer Slice(AudioBuffer buffer, double? startSec, double? endSec);
    Stream ToWavStream(AudioBuffer buffer);
}

public interface IReviewedStateService
{
    Task<Dictionary<string, Ams.Web.Shared.ValidationViewer.ReviewedStatusDto>> GetAsync(string bookId, CancellationToken ct = default);
    Task<Dictionary<string, Ams.Web.Shared.ValidationViewer.ReviewedStatusDto>> SetAsync(string bookId, string chapterId, bool reviewed, CancellationToken ct = default);
    Task ResetAsync(string bookId, CancellationToken ct = default);
}

public interface ICrxService
{
    Task<object> ExportAsync(string bookId, string chapterId, double start, double end, CancellationToken ct = default);
    Task<object> AddToCrxAsync(string bookId, string chapterId, JsonElement payload, CancellationToken ct = default);
}
