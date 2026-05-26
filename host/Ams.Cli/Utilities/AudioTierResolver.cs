using Ams.Cli.Repl;
using Ams.Core.Common;

namespace Ams.Cli.Utilities;

internal enum AudioTier
{
    Source,
    Treated,
    Filtered,
    Adjusted
}

internal static class AudioTierResolver
{
    public const string TierHelp = "Audio tier: source, treated, filtered";
    public const string StageTierHelp = "Audio tier to stage: source, treated, filtered, adjusted";

    private static readonly string[] VariantMarkers =
    [
        ".dsp.filtered",
        ".pause-adjusted",
        ".corrected",
        ".treated",
        ".filtered",
        ".dsp"
    ];

    public static AudioTier Parse(string? value, AudioTier defaultTier, bool allowAdjusted = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultTier;
        }

        var normalized = value.Trim().ToLowerInvariant();
        var tier = normalized switch
        {
            "source" or "raw" => AudioTier.Source,
            "treated" => AudioTier.Treated,
            "filtered" => AudioTier.Filtered,
            "adjusted" or "pause-adjusted" => AudioTier.Adjusted,
            _ => throw new ArgumentException(
                $"Unsupported audio tier '{value}'. Expected source, treated, filtered{(allowAdjusted ? ", or adjusted" : string.Empty)}.",
                nameof(value))
        };

        if (tier == AudioTier.Adjusted && !allowAdjusted)
        {
            throw new ArgumentException(
                "The adjusted audio tier is only supported by staging. Expected source, treated, or filtered.",
                nameof(value));
        }

        return tier;
    }

    public static string Describe(AudioTier tier) => tier switch
    {
        AudioTier.Source => "source",
        AudioTier.Treated => "treated",
        AudioTier.Filtered => "filtered",
        AudioTier.Adjusted => "pause-adjusted",
        _ => tier.ToString().ToLowerInvariant()
    };

    public static string? ArtifactSuffix(AudioTier tier) => tier switch
    {
        AudioTier.Source => null,
        AudioTier.Treated => "treated.wav",
        AudioTier.Filtered => "filtered.wav",
        AudioTier.Adjusted => "pause-adjusted.wav",
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null)
    };

    public static FileInfo ResolveInput(FileInfo? provided, AudioTier tier)
    {
        if (provided is not null)
        {
            return AmsPathResolver.NormalizeFile(provided);
        }

        return ResolveActiveTierFile(tier, mustExist: true);
    }

    public static FileInfo ResolveActiveTierFile(AudioTier tier, bool mustExist)
    {
        var context = ReplContext.Current;
        if (context is null)
        {
            throw new InvalidOperationException(
                "Audio file is required. Provide an explicit path or select a chapter with 'use'.");
        }

        if (tier == AudioTier.Source)
        {
            return context.ActiveChapter
                   ?? throw new InvalidOperationException("No active chapter. Use 'use' or 'mode all'.");
        }

        var suffix = ArtifactSuffix(tier)
                     ?? throw new InvalidOperationException("Source tier does not have an artifact suffix.");
        return context.ResolveChapterFile(suffix, mustExist);
    }

    public static bool IsVariantFileName(string fileName)
    {
        var stem = Path.GetFileNameWithoutExtension(fileName);
        return VariantMarkers.Any(marker => stem.EndsWith(marker, StringComparison.OrdinalIgnoreCase));
    }

    public static string StripVariantMarkers(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var stem = Path.GetFileNameWithoutExtension(fileName);

        bool removed;
        do
        {
            removed = false;
            foreach (var marker in VariantMarkers)
            {
                if (!stem.EndsWith(marker, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                stem = stem[..^marker.Length];
                removed = true;
                break;
            }
        } while (removed);

        return stem + extension;
    }
}
