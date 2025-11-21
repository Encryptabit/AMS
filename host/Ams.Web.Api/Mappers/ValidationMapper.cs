using Ams.Web.Api.Dtos.Validation;
using Ams.Web.Api.Payloads;
using Riok.Mapperly.Abstractions;

namespace Ams.Web.Api.Mappers;

[Mapper]
public partial class ValidationMapper
{
    public partial ValidationChapterSummaryDto Map(ChapterSummaryResponse source);
}
