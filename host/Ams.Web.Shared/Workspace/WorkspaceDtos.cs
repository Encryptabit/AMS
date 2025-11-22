namespace Ams.Web.Shared.Workspace;

public sealed record WorkspaceRequest(
    string? WorkspaceRoot,
    string? BookIndexPath,
    string? CrxTemplatePath,
    string? CrxDirectoryName,
    string? DefaultErrorType);

public sealed record WorkspaceResponse(
    string? WorkspaceRoot,
    string? BookIndexPath,
    string? CrxTemplatePath,
    string CrxDirectoryName,
    string DefaultErrorType,
    string? CurrentBookId);
