using System.Globalization;
using System.IO;
using System.Text.Json;
using Ams.Core.Artifacts;

namespace Ams.Core.Application.Benchmark;

public sealed class BenchmarkHydrateTimingReadException : Exception
{
    public BenchmarkHydrateTimingReadException(string hydratePath, string message, Exception? innerException = null)
        : base($"Hydrate timing parse failure for '{NormalizePath(hydratePath)}': {message}", innerException)
    {
        HydratePath = NormalizePath(hydratePath);
    }

    public string HydratePath { get; }

    private static string NormalizePath(string hydratePath)
    {
        if (string.IsNullOrWhiteSpace(hydratePath))
        {
            return "unknown";
        }

        return hydratePath.Trim().Replace('\\', '/');
    }
}

public static class BenchmarkHydrateTimingReader
{
    public static IReadOnlyDictionary<int, SentenceTiming> ReadLenient(string hydratePath)
    {
        var normalizedPath = EnsureHydratePath(hydratePath);

        using var document = ParseDocument(normalizedPath);
        if (!TryGetSentencesArray(document.RootElement, out var sentences))
        {
            return new Dictionary<int, SentenceTiming>();
        }

        var timings = new Dictionary<int, SentenceTiming>();
        foreach (var sentence in sentences.EnumerateArray())
        {
            if (!TryReadSentenceTiming(sentence, out var sentenceId, out var timing, out _))
            {
                continue;
            }

            timings[sentenceId] = timing;
        }

        return timings;
    }

    public static IReadOnlyDictionary<int, SentenceTiming> ReadStrict(string hydratePath)
    {
        var normalizedPath = EnsureHydratePath(hydratePath);

        using var document = ParseDocument(normalizedPath);
        if (!TryGetSentencesArray(document.RootElement, out var sentences))
        {
            throw new BenchmarkHydrateTimingReadException(normalizedPath,
                "Hydrate payload missing required 'sentences' array.");
        }

        var timings = new Dictionary<int, SentenceTiming>();
        var index = 0;

        foreach (var sentence in sentences.EnumerateArray())
        {
            index++;

            if (!TryReadSentenceTiming(sentence, out var sentenceId, out var timing, out var reason))
            {
                throw new BenchmarkHydrateTimingReadException(normalizedPath,
                    $"Sentence entry #{index} is malformed: {reason}");
            }

            if (!timings.TryAdd(sentenceId, timing))
            {
                throw new BenchmarkHydrateTimingReadException(normalizedPath,
                    $"Duplicate sentence id '{sentenceId}' detected.");
            }
        }

        if (timings.Count == 0)
        {
            throw new BenchmarkHydrateTimingReadException(normalizedPath,
                "Hydrate payload contains an empty 'sentences' array.");
        }

        return timings;
    }

    private static string EnsureHydratePath(string hydratePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hydratePath);

        var normalizedPath = hydratePath.Trim();
        if (!File.Exists(normalizedPath))
        {
            throw new FileNotFoundException(
                $"Hydrate timing file was not found: {normalizedPath}",
                normalizedPath);
        }

        return normalizedPath;
    }

    private static JsonDocument ParseDocument(string hydratePath)
    {
        try
        {
            using var stream = File.OpenRead(hydratePath);
            return JsonDocument.Parse(stream);
        }
        catch (BenchmarkHydrateTimingReadException)
        {
            throw;
        }
        catch (JsonException ex)
        {
            throw new BenchmarkHydrateTimingReadException(hydratePath,
                "JSON payload is invalid.", ex);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new BenchmarkHydrateTimingReadException(hydratePath,
                ex.Message,
                ex);
        }
    }

    private static bool TryGetSentencesArray(JsonElement root, out JsonElement sentences)
    {
        sentences = default;

        return root.ValueKind == JsonValueKind.Object
               && root.TryGetProperty("sentences", out sentences)
               && sentences.ValueKind == JsonValueKind.Array;
    }

    private static bool TryReadSentenceTiming(
        JsonElement sentence,
        out int sentenceId,
        out SentenceTiming timing,
        out string reason)
    {
        sentenceId = 0;
        timing = default!;
        reason = string.Empty;

        if (!TryGetInt(sentence, "id", out sentenceId))
        {
            reason = "Missing or invalid 'id'.";
            return false;
        }

        if (!TryReadTimingRange(sentence, out var startSec, out var endSec))
        {
            reason = "Missing or invalid timing fields (expected start/end or startSec/endSec).";
            return false;
        }

        if (!double.IsFinite(startSec) || !double.IsFinite(endSec))
        {
            reason = "Timing values must be finite numbers.";
            return false;
        }

        if (startSec < 0 || endSec < 0)
        {
            reason = "Timing values cannot be negative.";
            return false;
        }

        if (endSec < startSec)
        {
            reason = "Timing end is before start.";
            return false;
        }

        timing = new SentenceTiming(startSec, endSec);
        return true;
    }

    private static bool TryReadTimingRange(JsonElement sentence, out double startSec, out double endSec)
    {
        startSec = double.NaN;
        endSec = double.NaN;

        if (sentence.TryGetProperty("timing", out var timingNode) && timingNode.ValueKind == JsonValueKind.Object)
        {
            if (TryReadTimingObject(timingNode, out startSec, out endSec))
            {
                return true;
            }
        }

        return TryReadTimingObject(sentence, out startSec, out endSec);
    }

    private static bool TryReadTimingObject(JsonElement node, out double startSec, out double endSec)
    {
        startSec = double.NaN;
        endSec = double.NaN;

        if (TryGetDouble(node, "startSec", out startSec) && TryGetDouble(node, "endSec", out endSec))
        {
            return true;
        }

        if (TryGetDouble(node, "start", out startSec) && TryGetDouble(node, "end", out endSec))
        {
            return true;
        }

        return false;
    }

    private static bool TryGetInt(JsonElement element, string propertyName, out int value)
    {
        value = 0;

        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
        {
            return true;
        }

        if (property.ValueKind == JsonValueKind.String
            && int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        return false;
    }

    private static bool TryGetDouble(JsonElement element, string propertyName, out double value)
    {
        value = double.NaN;

        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number)
        {
            value = property.GetDouble();
            return true;
        }

        if (property.ValueKind == JsonValueKind.String
            && double.TryParse(property.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            value = parsed;
            return true;
        }

        return false;
    }
}
