namespace Ams.Core.Artifacts;

/// <summary>
/// Represents an absolute time span in seconds with microsecond precision.
/// </summary>
public record TimingRange
{
    private const double Precision = 1e-6;

    public double StartSec { get; }
    public double EndSec { get; }

    public double Duration => EndSec - StartSec;

    public static TimingRange Empty { get; } = new(0d, 0d);

    public TimingRange(double startSec, double endSec)
    {
        if (double.IsNaN(startSec) || double.IsNaN(endSec))
            throw new ArgumentException("Timing values cannot be NaN.");

        if (double.IsInfinity(startSec) || double.IsInfinity(endSec))
            throw new ArgumentException("Timing values cannot be infinite.");

        if (endSec + Precision < startSec)
        {
            endSec = startSec;
        }

        StartSec = Round(startSec);
        EndSec = Round(Math.Max(startSec, endSec));
    }

    public TimingRange WithEnd(double endSec) => new(StartSec, endSec);

    public TimingRange WithStart(double startSec) => new(startSec, EndSec);

    private static double Round(double value) => Math.Round(value, 6, MidpointRounding.AwayFromZero);
}