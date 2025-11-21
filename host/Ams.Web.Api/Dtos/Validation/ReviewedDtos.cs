namespace Ams.Web.Api.Dtos.Validation;

public sealed record ReviewedStatusDto(bool Reviewed, string? TimestampUtc);

public sealed record ReviewedStatusResponse(Dictionary<string, ReviewedStatusDto> Chapters);
