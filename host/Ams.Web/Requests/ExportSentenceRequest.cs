namespace Ams.Web.Requests;

public sealed record ExportSentenceRequest
{
    public string? ErrorType { get; init; }
    public string? Comment { get; init; }
}
