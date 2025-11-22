using System.Text.Json.Serialization;
using Ams.Web.Api.Payloads;
using Ams.Web.Shared.Validation;
using Ams.Web.Shared.Workspace;
using PayloadWorkspaceRequest = Ams.Web.Shared.Workspace.WorkspaceRequest;
using PayloadWorkspaceResponse = Ams.Web.Shared.Workspace.WorkspaceResponse;
using ValidationChapterSummaryDto = Ams.Web.Shared.Validation.ValidationChapterSummaryDto;
using ChapterSummaryResponse = Ams.Web.Api.Payloads.ChapterSummaryResponse;
using ChapterDetailDto = Ams.Web.Shared.Validation.ChapterDetailDto;
using SentenceDto = Ams.Web.Shared.Validation.SentenceDto;
using ParagraphDto = Ams.Web.Shared.Validation.ParagraphDto;
using AudioAvailabilityDto = Ams.Web.Shared.Validation.AudioAvailabilityDto;
using AudioExportResponse = Ams.Web.Shared.Validation.AudioExportResponse;
using TimingDto = Ams.Web.Shared.Validation.TimingDto;
using RangeDto = Ams.Web.Shared.Validation.RangeDto;
using MetricsDto = Ams.Web.Shared.Validation.MetricsDto;
using ParagraphMetricsDto = Ams.Web.Shared.Validation.ParagraphMetricsDto;
using DiffDto = Ams.Web.Shared.Validation.DiffDto;
using DiffOpDto = Ams.Web.Shared.Validation.DiffOpDto;
using DiffStatsDto = Ams.Web.Shared.Validation.DiffStatsDto;
using AudioExportRequest = Ams.Web.Shared.Validation.AudioExportRequest;

namespace Ams.Web.Api.Json;

// Central serializer context for minimal APIs (reflection disabled).
[JsonSerializable(typeof(PayloadWorkspaceRequest))]
[JsonSerializable(typeof(PayloadWorkspaceResponse))]
[JsonSerializable(typeof(ChapterSummaryResponse[]))]
[JsonSerializable(typeof(IEnumerable<ChapterSummaryResponse>))]
[JsonSerializable(typeof(IEnumerable<PayloadWorkspaceResponse>))]
[JsonSerializable(typeof(ValidationChapterSummaryDto[]))]
[JsonSerializable(typeof(IEnumerable<ValidationChapterSummaryDto>))]
[JsonSerializable(typeof(List<ValidationChapterSummaryDto>))]
[JsonSerializable(typeof(List<ChapterSummaryResponse>))]
[JsonSerializable(typeof(List<PayloadWorkspaceResponse>))]
[JsonSerializable(typeof(ChapterDetailDto))]
[JsonSerializable(typeof(IEnumerable<ChapterDetailDto>))]
[JsonSerializable(typeof(SentenceDto[]))]
[JsonSerializable(typeof(IEnumerable<SentenceDto>))]
[JsonSerializable(typeof(ParagraphDto[]))]
[JsonSerializable(typeof(IEnumerable<ParagraphDto>))]
[JsonSerializable(typeof(AudioAvailabilityDto))]
[JsonSerializable(typeof(AudioExportResponse))]
[JsonSerializable(typeof(AudioExportRequest))]
[JsonSerializable(typeof(TimingDto))]
[JsonSerializable(typeof(RangeDto))]
[JsonSerializable(typeof(MetricsDto))]
[JsonSerializable(typeof(ParagraphMetricsDto))]
[JsonSerializable(typeof(DiffDto))]
[JsonSerializable(typeof(DiffOpDto))]
[JsonSerializable(typeof(DiffStatsDto))]
[JsonSerializable(typeof(IEnumerable<DiffOpDto>))]
[JsonSerializable(typeof(IEnumerable<DiffDto>))]
[JsonSerializable(typeof(ValidationOverviewDto))]
[JsonSerializable(typeof(ValidationReportDto))]
[JsonSerializable(typeof(ReviewedStatusDto))]
[JsonSerializable(typeof(ReviewedStatusResponse))]
[JsonSerializable(typeof(Dictionary<string, ReviewedStatusDto>))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<string, ReviewedStatusDto>>), TypeInfoPropertyName = "ReviewedStore")]
[JsonSerializable(typeof(IEnumerable<ReviewedStatusDto>))]
[JsonSerializable(typeof(IEnumerable<ValidationOverviewDto>))]
[JsonSerializable(typeof(IEnumerable<ValidationReportDto>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ValidationProblemDetails))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
[JsonSerializable(typeof(IDictionary<string, string[]>))]
internal partial class ApiJsonSerializerContext : JsonSerializerContext;
