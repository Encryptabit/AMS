namespace Ams.Web.Shared.Extensions;

public static class ActivitySourceExtensions
{
    private static readonly ActivitySource _current =
        new("Ams.Web", typeof(ActivitySourceExtensions).Assembly.GetName().Version?.ToString() ?? "1.0.0");

    /// <summary>Open telemetry activity source for the application.</summary>
    public static ActivitySource Current => _current;
}
