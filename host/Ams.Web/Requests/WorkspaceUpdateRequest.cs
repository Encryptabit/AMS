namespace Ams.Web.Requests;

public sealed record WorkspaceUpdateRequest
{
    public string? WorkspaceRoot { get; init; }
    public string? BookIndexPath { get; init; }
    public string? CrxTemplatePath { get; init; }
}
