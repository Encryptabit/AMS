using System;

namespace Ams.Core.Common;

public static class LevenshteinMetrics
{
    public static int Distance(string a, string b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return Distance(a.AsSpan(), b.AsSpan());
    }

    public static int Distance(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
    {
        if (a.IsEmpty)
        {
            return b.Length;
        }

        if (b.IsEmpty)
        {
            return a.Length;
        }

        var dp = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++)
        {
            dp[i, 0] = i;
        }

        for (int j = 0; j <= b.Length; j++)
        {
            dp[0, j] = j;
        }

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                dp[i, j] = Math.Min(
                    Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                    dp[i - 1, j - 1] + cost);
            }
        }

        return dp[a.Length, b.Length];
    }

    public static int Distance(ReadOnlySpan<string> a, ReadOnlySpan<string> b, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (a.IsEmpty)
        {
            return b.Length;
        }

        if (b.IsEmpty)
        {
            return a.Length;
        }

        var dp = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++)
        {
            dp[i, 0] = i;
        }

        for (int j = 0; j <= b.Length; j++)
        {
            dp[0, j] = j;
        }

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                var cost = string.Equals(a[i - 1], b[j - 1], comparisonType) ? 0 : 1;
                dp[i, j] = Math.Min(
                    Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                    dp[i - 1, j - 1] + cost);
            }
        }

        return dp[a.Length, b.Length];
    }

    public static double Similarity(ReadOnlySpan<string> a, ReadOnlySpan<string> b, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (a.IsEmpty && b.IsEmpty)
        {
            return 1.0;
        }

        if (a.IsEmpty || b.IsEmpty)
        {
            return 0.0;
        }

        var maxLen = Math.Max(a.Length, b.Length);
        if (maxLen == 0)
        {
            return 1.0;
        }

        var distance = Distance(a, b, comparisonType);
        return 1.0 - (double)distance / maxLen;
    }

    public static double Similarity(string a, string b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        if (a.Length == 0 && b.Length == 0)
        {
            return 1.0;
        }

        if (a.Length == 0 || b.Length == 0)
        {
            return 0.0;
        }

        var distance = Distance(a, b);
        var maxLen = Math.Max(a.Length, b.Length);
        return 1.0 - (double)distance / maxLen;
    }
}
