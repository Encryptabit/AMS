using Whisper.net.Ggml;

namespace Ams.Core.Asr;

// Whitelisted ASR models for AMS pipeline recovery decisions. Wraps Whisper.net's GgmlType so
// that the recovery cross-pair table and CLI parsing live in this repo. Custom .bin files are
// still supported via AMS_WHISPER_MODEL_PATH (which bypasses this enum entirely).
public enum AmsAsrModel
{
    LargeV3,
    LargeV3Turbo
}

public static class AmsAsrModelExtensions
{
    // Cross-pair table used by the recovery orchestrator's AlternateModel tier. The pair is
    // intentionally symmetric: a chapter that misaligns under one model often aligns cleanly
    // under the other.
    private static readonly Dictionary<AmsAsrModel, AmsAsrModel> CrossPair = new()
    {
        [AmsAsrModel.LargeV3] = AmsAsrModel.LargeV3Turbo,
        [AmsAsrModel.LargeV3Turbo] = AmsAsrModel.LargeV3
    };

    public static string ToAlias(this AmsAsrModel model) => model switch
    {
        AmsAsrModel.LargeV3 => "large-v3",
        AmsAsrModel.LargeV3Turbo => "large-v3-turbo",
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
    };

    public static GgmlType ToGgmlType(this AmsAsrModel model) => model switch
    {
        AmsAsrModel.LargeV3 => GgmlType.LargeV3,
        AmsAsrModel.LargeV3Turbo => GgmlType.LargeV3Turbo,
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
    };

    public static AmsAsrModel? GetDefaultFallback(this AmsAsrModel model)
        => CrossPair.TryGetValue(model, out var pair) ? pair : null;

    // Strict parse used by the CLI surface — throws on any value outside the whitelist. Empty
    // input returns null (signals "use environment / config default" downstream).
    public static AmsAsrModel? ParseStrict(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (TryParse(value, out var model))
        {
            return model;
        }

        throw new ArgumentException(
            $"Unrecognized ASR model '{value}'. Supported values: large-v3, large-v3-turbo. " +
            "Use AMS_WHISPER_MODEL_PATH to point at an arbitrary .bin file.",
            nameof(value));
    }

    // Lenient parse used by recovery cross-pair lookups for callers that may pass arbitrary
    // strings (Workstation freeform field, env-var paths). Returns null when the value is not
    // one of the whitelisted aliases — caller decides what to do with that (typically: skip
    // AlternateModel tier).
    public static AmsAsrModel? TryParseLenient(string? value)
        => TryParse(value, out var model) ? model : null;

    private static bool TryParse(string? value, out AmsAsrModel model)
    {
        model = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().ToLowerInvariant()
            .Replace(".bin", string.Empty)
            .Replace("ggml-", string.Empty);

        switch (normalized)
        {
            case "large-v3":
                model = AmsAsrModel.LargeV3;
                return true;
            case "large-v3-turbo":
                model = AmsAsrModel.LargeV3Turbo;
                return true;
            default:
                return false;
        }
    }
}
