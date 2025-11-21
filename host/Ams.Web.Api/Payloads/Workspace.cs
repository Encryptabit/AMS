namespace Ams.Web.Api.Payloads;

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
    string DefaultErrorType);
