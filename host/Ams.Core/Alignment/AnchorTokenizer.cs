using System;

namespace Ams.Core.Alignment.Anchors;

/// <summary>
/// Canonical token normalization for anchor discovery. Lowercase and keep only letters/digits.
/// Produces a single stable token per input word; empty when the input is punctuation-only.
/// </summary>
public static class AnchorTokenizer
{
    public static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        var span = s.AsSpan();
        Span<char> buf = stackalloc char[s.Length];
        int k = 0;
        for (int i = 0; i < span.Length; i++)
        {
            char c = span[i];
            if (char.IsLetterOrDigit(c))
            {
                buf[k++] = char.ToLowerInvariant(c);
            }
        }
        return k == 0 ? string.Empty : new string(buf[..k]);
    }
}



