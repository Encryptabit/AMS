using Ams.Core.Common;
using Whisper.net.Ggml;

namespace Ams.Core.Asr;

public enum AsrEngine
{
    Whisper,
    WhisperX
}

public static class AsrEngineConfig
{
    public const string EngineEnvironmentVariable = "AMS_ASR_ENGINE";
    public const string WhisperModelPathEnvironmentVariable = "AMS_WHISPER_MODEL_PATH";
    public const string WhisperXExecutableEnvironmentVariable = "AMS_WHISPERX_BIN";
    public const string DefaultWhisperXModel = "large-v3";

    /// <summary>Default Whisper model type when none is specified.</summary>
    public static GgmlType DefaultModelType => GgmlType.LargeV3Turbo;

    public static AsrEngine Resolve(string? engineOption = null)
    {
        var value = engineOption ?? Environment.GetEnvironmentVariable(EngineEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(value))
        {
            return AsrEngine.Whisper;
        }

        value = value.Trim().ToLowerInvariant();
        return value switch
        {
            "whisper" => AsrEngine.Whisper,
            "whispernet" => AsrEngine.Whisper,
            "whisper.net" => AsrEngine.Whisper,
            "whisperx" => AsrEngine.WhisperX,
            "whisper-x" => AsrEngine.WhisperX,
            _ => AsrEngine.Whisper
        };
    }

    public static bool IsWhisper(string? engineOption = null) => Resolve(engineOption) == AsrEngine.Whisper;

    public static bool IsWhisperX(string? engineOption = null) => Resolve(engineOption) == AsrEngine.WhisperX;

    /// <summary>
    /// Synchronous model path resolution from explicit value or environment variable.
    /// Throws if neither is available. Prefer <see cref="ResolveModelPathAsync"/> which
    /// supports auto-download as a fallback.
    /// </summary>
    public static string ResolveModelPath(string? optionValue)
    {
        if (!string.IsNullOrWhiteSpace(optionValue))
        {
            return Path.GetFullPath(optionValue);
        }

        var env = Environment.GetEnvironmentVariable(WhisperModelPathEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(env))
        {
            return Path.GetFullPath(env.Trim());
        }

        throw new InvalidOperationException(
            "Whisper model path is required. Provide --model-path or set AMS_WHISPER_MODEL_PATH.");
    }

    /// <summary>
    /// Resolves the Whisper model path with full fallback chain:
    /// explicit path → model alias → environment variable → auto-download default.
    /// Downloads the model if it doesn't exist on disk.
    /// </summary>
    /// <param name="modelAlias">Optional model alias (e.g. "large-v3", "base").</param>
    /// <param name="modelPath">Optional explicit path to a .bin/.gguf model file.</param>
    /// <returns>The resolved model path and inferred model type.</returns>
    public static async Task<(string Path, GgmlType Type)> ResolveModelPathAsync(
        string? modelAlias = null,
        FileInfo? modelPath = null)
    {
        // 1. Explicit file path
        if (modelPath is not null)
        {
            var fullPath = Path.GetFullPath(modelPath.FullName);
            if (!File.Exists(fullPath))
            {
                var type = ParseModelAlias(modelAlias) ?? ParseModelAlias(Path.GetFileName(fullPath)) ??
                    DefaultModelType;
                var downloaded = await DownloadModelIfMissingAsync(fullPath, type).ConfigureAwait(false);
                return (downloaded, type);
            }

            var inferred = ParseModelAlias(Path.GetFileName(fullPath)) ?? DefaultModelType;
            return (fullPath, inferred);
        }

        // 2. Model alias (could be a path or a known alias like "large-v3")
        if (!string.IsNullOrWhiteSpace(modelAlias))
        {
            var trimmed = modelAlias.Trim();
            if (File.Exists(trimmed))
            {
                var inferred = ParseModelAlias(Path.GetFileName(trimmed)) ?? DefaultModelType;
                return (Path.GetFullPath(trimmed), inferred);
            }

            if (TryParseModelAlias(trimmed, out var aliasType))
            {
                var downloaded = await DownloadModelIfMissingAsync(null, aliasType).ConfigureAwait(false);
                return (downloaded, aliasType);
            }
        }

        // 3. Environment variable
        var envModel = Environment.GetEnvironmentVariable(WhisperModelPathEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(envModel))
        {
            var envPath = Path.GetFullPath(envModel);
            if (File.Exists(envPath))
            {
                var inferred = ParseModelAlias(Path.GetFileName(envPath)) ?? DefaultModelType;
                return (envPath, inferred);
            }

            var envType = ParseModelAlias(Path.GetFileName(envPath)) ?? ParseModelAlias(envModel) ??
                DefaultModelType;
            var downloaded = await DownloadModelIfMissingAsync(envPath, envType).ConfigureAwait(false);
            return (downloaded, envType);
        }

        // 4. Auto-download default model
        var defaultPath = await DownloadModelIfMissingAsync(null, DefaultModelType)
            .ConfigureAwait(false);
        return (defaultPath, DefaultModelType);
    }

    /// <summary>
    /// Downloads a Whisper GGML model if it does not already exist at the target path.
    /// </summary>
    internal static async Task<string> DownloadModelIfMissingAsync(string? destinationPath, GgmlType type)
    {
        var fileName = GetDefaultModelFileName(type);
        var targetPath = destinationPath ?? Path.Combine(AppContext.BaseDirectory, "models", fileName);
        targetPath = Path.GetFullPath(targetPath);

        if (File.Exists(targetPath))
        {
            Log.Debug("Using cached Whisper model at {ModelPath}", targetPath);
            return targetPath;
        }

        var directory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Log.Info("Downloading Whisper model {ModelType} to {ModelPath}", type, targetPath);
        await using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(type)
            .ConfigureAwait(false);
        await using var fileWriter = File.Open(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await modelStream.CopyToAsync(fileWriter).ConfigureAwait(false);
        Log.Info("Whisper model ready at {ModelPath}", targetPath);

        return targetPath;
    }

    internal static string GetDefaultModelFileName(GgmlType type)
    {
        var suffix = type switch
        {
            GgmlType.Base => "base",
            GgmlType.Small => "small",
            GgmlType.Medium => "medium",
            GgmlType.LargeV1 => "large-v1",
            GgmlType.LargeV2 => "large-v2",
            GgmlType.LargeV3 => "large-v3",
            GgmlType.LargeV3Turbo => "large-v3-turbo",
            _ => "base"
        };

        return $"ggml-{suffix}.bin";
    }

    /// <summary>
    /// Parses a model alias string (e.g. "large-v3", "base.en") to a <see cref="GgmlType"/>.
    /// Returns null if the string is not a recognized alias.
    /// </summary>
    public static GgmlType? ParseModelAlias(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim().ToLowerInvariant()
            .Replace(".bin", string.Empty)
            .Replace("ggml-", string.Empty);
        return trimmed switch
        {
            "tiny" => GgmlType.Tiny,
            "tiny.en" => GgmlType.TinyEn,
            "base" => GgmlType.Base,
            "base.en" => GgmlType.BaseEn,
            "small" => GgmlType.Small,
            "small.en" => GgmlType.SmallEn,
            "medium" => GgmlType.Medium,
            "medium.en" => GgmlType.MediumEn,
            "large" or "large-v1" => GgmlType.LargeV1,
            "large-v2" => GgmlType.LargeV2,
            "large-v3" => GgmlType.LargeV3,
            "large-v3-turbo" => GgmlType.LargeV3Turbo,
            _ => null
        };
    }

    internal static bool TryParseModelAlias(string value, out GgmlType type)
    {
        var parsed = ParseModelAlias(value);
        if (parsed.HasValue)
        {
            type = parsed.Value;
            return true;
        }

        type = DefaultModelType;
        return false;
    }
}
