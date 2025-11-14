namespace Ams.Web.Dtos;

public sealed record WorkspaceStateDto(
    string WorkspaceRoot,
    string BookIndexPath,
    string CrxTemplatePath);
