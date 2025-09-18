using System.Globalization;

namespace Ams.Core.Util;

public static class Precision
{
    private const int MicrosecondDigits = 6;

    public static double RoundToMicroseconds(double value) => System.Math.Round(value, MicrosecondDigits, MidpointRounding.AwayFromZero);

    public static string ToInvariantMicroseconds(double value) => RoundToMicroseconds(value).ToString("F6", CultureInfo.InvariantCulture);
}
