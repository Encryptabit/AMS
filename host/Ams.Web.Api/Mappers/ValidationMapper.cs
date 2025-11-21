using Ams.Web.Api.Dtos.Validation;
using Riok.Mapperly.Abstractions;

namespace Ams.Web.Api.Mappers;

[Mapper]
public partial class ValidationMapper
{
    public partial ValidationChapterSummaryDto Map(ChapterSummaryResponse source);
}
