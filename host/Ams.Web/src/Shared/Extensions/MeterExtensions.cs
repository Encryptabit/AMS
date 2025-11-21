namespace System.Diagnostics.Metrics;

public static class MeterExtensions
{
    private static readonly Meter current =
        new("Ams.Web", typeof(MeterExtensions).Assembly.GetName().Version?.ToString() ?? "1.0.0");

    /// <summary>Open telemetry meter for the application.</summary>
    public static Meter Current => current;
}

