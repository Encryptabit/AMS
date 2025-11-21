using System.Text.Json.Serialization;
using Ams.Web.Api.Dtos.Validation;
using Ams.Web.Api.Payloads;
using PayloadWorkspaceRequest = Ams.Web.Api.Payloads.WorkspaceRequest;
using PayloadWorkspaceResponse = Ams.Web.Api.Payloads.WorkspaceResponse;
using ValidationChapterSummaryDto = Ams.Web.Api.Dtos.Validation.ValidationChapterSummaryDto;
using ChapterSummaryResponse = Ams.Web.Api.Dtos.Validation.ChapterSummaryResponse;
using ChapterDetailDto = Ams.Web.Api.Dtos.Validation.ChapterDetailDto;
using SentenceDto = Ams.Web.Api.Dtos.Validation.SentenceDto;
using ParagraphDto = Ams.Web.Api.Dtos.Validation.ParagraphDto;
using AudioAvailabilityDto = Ams.Web.Api.Dtos.Validation.AudioAvailabilityDto;
using AudioExportResponse = Ams.Web.Api.Dtos.Validation.AudioExportResponse;

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
