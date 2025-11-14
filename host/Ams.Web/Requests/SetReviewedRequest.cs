namespace Ams.Web.Requests;

public sealed record SetReviewedRequest
{
    public bool Reviewed { get; init; }
}
