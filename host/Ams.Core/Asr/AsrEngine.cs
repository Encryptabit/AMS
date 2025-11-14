namespace Ams.Core.Asr;

public enum AsrEngine
{
    Whisper,
    Nemo
}

public static class AsrEngineConfig
{
    public const string EngineEnvironmentVariable = "AMS_ASR_ENGINE";
    public const string WhisperModelPathEnvironmentVariable = "AMS_WHISPER_MODEL_PATH";

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
            "nemo" => AsrEngine.Nemo,
            "whisper" => AsrEngine.Whisper,
            "whispernet" => AsrEngine.Whisper,
            "whisper.net" => AsrEngine.Whisper,
            _ => AsrEngine.Whisper
        };
    }

    public static bool IsWhisper(string? engineOption = null) => Resolve(engineOption) == AsrEngine.Whisper;

    public static bool IsNemo(string? engineOption = null) => Resolve(engineOption) == AsrEngine.Nemo;

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

        throw new InvalidOperationException("Whisper model path is required. Provide --model-path or set AMS_WHISPER_MODEL_PATH.");
    }
}
