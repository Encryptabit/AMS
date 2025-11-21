using System.Text.Json.Serialization;
using Ams.Web.Api.Dtos;
using Ams.Web.Api.Dtos.Validation;

namespace Ams.Web.Api.Json;

// Central serializer context for minimal APIs (reflection disabled).
[JsonSerializable(typeof(WorkspaceRequest))]
[JsonSerializable(typeof(WorkspaceResponse))]
[JsonSerializable(typeof(ChapterSummaryResponse[]))]
[JsonSerializable(typeof(IEnumerable<ChapterSummaryResponse>))]
[JsonSerializable(typeof(IEnumerable<WorkspaceResponse>))]
[JsonSerializable(typeof(ValidationChapterSummaryDto[]))]
[JsonSerializable(typeof(IEnumerable<ValidationChapterSummaryDto>))]
[JsonSerializable(typeof(List<ValidationChapterSummaryDto>))]
[JsonSerializable(typeof(List<ChapterSummaryResponse>))]
[JsonSerializable(typeof(List<WorkspaceResponse>))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ValidationProblemDetails))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
[JsonSerializable(typeof(IDictionary<string, string[]>))]
internal partial class ApiJsonSerializerContext : JsonSerializerContext;
