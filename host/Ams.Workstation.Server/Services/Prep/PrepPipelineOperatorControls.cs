using System.Globalization;
using Ams.Core.Application.Commands;
using Ams.Core.Application.Mfa.Models;
using Ams.Core.Application.Pipeline;
using Ams.Core.Asr;

namespace Ams.Workstation.Server.Services.Prep;

public sealed class PrepPipelineOperatorControls
{
    public const string EngineAutoKey = "auto";
    public const string EngineWhisperKey = "whisper";
    public const string EngineWhisperXKey = "whisperx";

    public const string BeamProfileDefaultKey = "default";
    public const string BeamProfileFastKey = "fast";
    public const string BeamProfileBalancedKey = "balanced";
    public const string BeamProfileStrictKey = "strict";

    private PrepPipelineRunRequest _lastKnownValidRequest;

    public PrepPipelineOperatorControls()
    {
        _lastKnownValidRequest = PrepPipelineRunRequest.Default;
        ApplyRequest(_lastKnownValidRequest, updateLastKnownValid: false);
    }

    public static IReadOnlyList<OperatorControlOption> EndStageOptions { get; } =
    [
        new("book-index", "Book index"),
        new("asr", "ASR"),
        new("anchors", "Anchors"),
        new("transcript", "Transcript"),
        new("hydrate", "Hydrate"),
        new("mfa", "MFA")
    ];

    public static IReadOnlyList<OperatorControlOption> AsrEngineOptions { get; } =
    [
        new(EngineAutoKey, "Auto (workspace default)"),
        new(EngineWhisperKey, "Whisper"),
        new(EngineWhisperXKey, "WhisperX")
    ];

    public static IReadOnlyList<OperatorControlOption> MfaBeamProfileOptions { get; } =
    [
        new(BeamProfileDefaultKey, "Default (Balanced)"),
        new(BeamProfileFastKey, "Fast"),
        new(BeamProfileBalancedKey, "Balanced"),
        new(BeamProfileStrictKey, "Strict")
    ];

    public string EndStageKey { get; set; } = "mfa";

    public bool Force { get; set; }

    public bool ForceIndex { get; set; }

    public string AsrEngineKey { get; set; } = EngineAutoKey;

    public string AsrModel { get; set; } = string.Empty;

    public string AsrLanguage { get; set; } = GenerateTranscriptOptions.Default.Language;

    public bool EnableWordTimestamps { get; set; } = GenerateTranscriptOptions.Default.EnableWordTimestamps;

    public bool EnableFlashAttention { get; set; }

    public bool EnableDtwTimestamps { get; set; }

    public bool DisablePrompt { get; set; }

    public string MfaBeamProfileKey { get; set; } = BeamProfileDefaultKey;

    public string MfaBeamText { get; set; } = string.Empty;

    public string MfaRetryBeamText { get; set; } = string.Empty;

    public bool DisableChunkPlan { get; set; }

    public bool DisableChunkedMfa { get; set; }

    public bool RequireAsrChunkAudio { get; set; }

    public string ChunkSilenceThresholdDbText { get; set; } = string.Empty;

    public string ChunkMinSilenceDurationMsText { get; set; } = string.Empty;

    public string ChunkMinDurationSecText { get; set; } = string.Empty;

    public string ChunkMaxDurationSecText { get; set; } = string.Empty;

    public string? LastValidationMessage { get; private set; }

    public string? LastNormalizationWarning { get; private set; }

    public string? GetValidationMessage(string? selectedChapter, IReadOnlyList<string> availableChapters)
    {
        ArgumentNullException.ThrowIfNull(availableChapters);

        if (string.IsNullOrWhiteSpace(selectedChapter))
        {
            LastValidationMessage = "Select a chapter before running Prep pipeline.";
            return LastValidationMessage;
        }

        var chapterExists = availableChapters.Contains(selectedChapter, StringComparer.OrdinalIgnoreCase);
        if (!chapterExists)
        {
            LastValidationMessage = "The selected chapter is no longer available. Re-select a chapter before running Prep pipeline.";
            return LastValidationMessage;
        }

        if (!TryParseRequest(out _, out var errors))
        {
            LastValidationMessage = string.Join(" ", errors);
            return LastValidationMessage;
        }

        LastValidationMessage = null;
        return null;
    }

    public bool TryBuildRequest(out PrepPipelineRunRequest request)
    {
        if (!TryParseRequest(out request, out var errors))
        {
            var warning = $"Malformed option values were reverted to the last-known-valid selection: {string.Join(" ", errors)}";
            ApplyRequest(_lastKnownValidRequest, updateLastKnownValid: false);
            LastValidationMessage = string.Join(" ", errors);
            LastNormalizationWarning = warning;
            request = _lastKnownValidRequest;
            return false;
        }

        _lastKnownValidRequest = request;
        LastValidationMessage = null;
        LastNormalizationWarning = null;
        return true;
    }

    public IReadOnlyList<string> GetCompatibilityWarnings()
    {
        var warnings = new List<string>();

        var engine = ResolveAsrEngine(AsrEngineKey, new List<string>());
        if (engine == AsrEngine.WhisperX && EnableDtwTimestamps)
        {
            warnings.Add("WhisperX ignores DTW timestamps. DTW will be disabled when the run starts.");
        }

        if (engine == AsrEngine.WhisperX && EnableFlashAttention)
        {
            warnings.Add("WhisperX ignores FlashAttention. FlashAttention will be disabled when the run starts.");
        }

        if (DisableChunkedMfa && RequireAsrChunkAudio)
        {
            warnings.Add("Require ASR chunk audio is ignored when chunked MFA is disabled.");
        }

        if (!string.IsNullOrWhiteSpace(LastNormalizationWarning))
        {
            warnings.Add(LastNormalizationWarning);
        }

        return warnings;
    }

    public void ApplyRequest(PrepPipelineRunRequest request, bool updateLastKnownValid = true)
    {
        ArgumentNullException.ThrowIfNull(request);

        EndStageKey = StageToKey(request.EndStage);
        Force = request.Force;
        ForceIndex = request.ForceIndex;

        AsrEngineKey = request.Asr.Engine switch
        {
            AsrEngine.Whisper => EngineWhisperKey,
            AsrEngine.WhisperX => EngineWhisperXKey,
            _ => EngineAutoKey
        };

        AsrModel = request.Asr.Model ?? string.Empty;
        AsrLanguage = request.Asr.Language ?? GenerateTranscriptOptions.Default.Language;
        EnableWordTimestamps = request.Asr.EnableWordTimestamps;
        EnableFlashAttention = request.Asr.EnableFlashAttention;
        EnableDtwTimestamps = request.Asr.EnableDtwTimestamps;
        DisablePrompt = request.Asr.DisablePrompt;

        MfaBeamProfileKey = request.Mfa.BeamProfile switch
        {
            MfaBeamProfile.Fast => BeamProfileFastKey,
            MfaBeamProfile.Balanced => BeamProfileBalancedKey,
            MfaBeamProfile.Strict => BeamProfileStrictKey,
            _ => BeamProfileDefaultKey
        };

        MfaBeamText = request.Mfa.Beam?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        MfaRetryBeamText = request.Mfa.RetryBeam?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

        DisableChunkPlan = request.Chunking.DisableChunkPlan;
        DisableChunkedMfa = request.Chunking.DisableChunkedMfa;
        RequireAsrChunkAudio = request.Chunking.RequireAsrChunkAudio;

        ChunkSilenceThresholdDbText = request.Chunking.Policy.SilenceThresholdDb?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        ChunkMinSilenceDurationMsText = request.Chunking.Policy.MinSilenceDurationMs?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        ChunkMinDurationSecText = request.Chunking.Policy.MinChunkDurationSec?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        ChunkMaxDurationSecText = request.Chunking.Policy.MaxChunkDurationSec?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

        if (updateLastKnownValid)
        {
            _lastKnownValidRequest = request;
        }
    }

    private bool TryParseRequest(out PrepPipelineRunRequest request, out IReadOnlyList<string> errors)
    {
        var parseErrors = new List<string>();

        var endStage = ParseEndStage(EndStageKey, parseErrors);
        var asrEngine = ResolveAsrEngine(AsrEngineKey, parseErrors);
        var beamProfile = ResolveBeamProfile(MfaBeamProfileKey, parseErrors);

        var beam = ParseOptionalInt(MfaBeamText, "MFA beam", parseErrors);
        var retryBeam = ParseOptionalInt(MfaRetryBeamText, "MFA retry beam", parseErrors);

        var silenceThresholdDb = ParseOptionalDouble(ChunkSilenceThresholdDbText, "Chunk silence threshold", parseErrors);
        var minSilenceDurationMs = ParseOptionalDouble(ChunkMinSilenceDurationMsText, "Chunk minimum silence duration", parseErrors);
        var minDurationSec = ParseOptionalDouble(ChunkMinDurationSecText, "Chunk minimum duration", parseErrors);
        var maxDurationSec = ParseOptionalDouble(ChunkMaxDurationSecText, "Chunk maximum duration", parseErrors);

        errors = parseErrors;

        if (parseErrors.Count > 0)
        {
            request = _lastKnownValidRequest;
            return false;
        }

        request = new PrepPipelineRunRequest
        {
            EndStage = endStage,
            Force = Force,
            ForceIndex = ForceIndex,
            Asr = new PrepPipelineAsrRequest
            {
                Engine = asrEngine,
                Model = AsrModel,
                Language = AsrLanguage,
                EnableWordTimestamps = EnableWordTimestamps,
                EnableFlashAttention = EnableFlashAttention,
                EnableDtwTimestamps = EnableDtwTimestamps,
                DisablePrompt = DisablePrompt
            },
            Mfa = new PrepPipelineMfaRequest
            {
                BeamProfile = beamProfile,
                Beam = beam,
                RetryBeam = retryBeam
            },
            Chunking = new PrepPipelineChunkRequest
            {
                DisableChunkPlan = DisableChunkPlan,
                DisableChunkedMfa = DisableChunkedMfa,
                RequireAsrChunkAudio = RequireAsrChunkAudio,
                Policy = new PrepPipelineChunkPolicyRequest
                {
                    SilenceThresholdDb = silenceThresholdDb,
                    MinSilenceDurationMs = minSilenceDurationMs,
                    MinChunkDurationSec = minDurationSec,
                    MaxChunkDurationSec = maxDurationSec
                }
            }
        };

        return true;
    }

    private static PipelineStage ParseEndStage(string? key, List<string> errors)
    {
        var normalized = key?.Trim().ToLowerInvariant();

        return normalized switch
        {
            "book-index" => PipelineStage.BookIndex,
            "asr" => PipelineStage.Asr,
            "anchors" => PipelineStage.Anchors,
            "transcript" => PipelineStage.Transcript,
            "hydrate" => PipelineStage.Hydrate,
            "mfa" => PipelineStage.Mfa,
            _ => AddErrorAndReturnDefault(errors, "Pipeline end stage is invalid.", PipelineStage.Mfa)
        };
    }

    private static AsrEngine? ResolveAsrEngine(string? key, List<string> errors)
    {
        var normalized = key?.Trim().ToLowerInvariant();

        return normalized switch
        {
            EngineAutoKey => null,
            EngineWhisperKey => AsrEngine.Whisper,
            EngineWhisperXKey => AsrEngine.WhisperX,
            _ => AddErrorAndReturnDefault(errors, "ASR engine selection is invalid.", default(AsrEngine?))
        };
    }

    private static MfaBeamProfile? ResolveBeamProfile(string? key, List<string> errors)
    {
        var normalized = key?.Trim().ToLowerInvariant();

        return normalized switch
        {
            BeamProfileDefaultKey => null,
            BeamProfileFastKey => MfaBeamProfile.Fast,
            BeamProfileBalancedKey => MfaBeamProfile.Balanced,
            BeamProfileStrictKey => MfaBeamProfile.Strict,
            _ => AddErrorAndReturnDefault(errors, "MFA beam profile selection is invalid.", default(MfaBeamProfile?))
        };
    }

    private static int? ParseOptionalInt(string? value, string fieldName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        errors.Add($"{fieldName} must be a whole number or blank.");
        return null;
    }

    private static double? ParseOptionalDouble(string? value, string fieldName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (double.TryParse(value.Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        errors.Add($"{fieldName} must be a number or blank.");
        return null;
    }

    private static string StageToKey(PipelineStage stage)
        => stage switch
        {
            PipelineStage.BookIndex => "book-index",
            PipelineStage.Asr => "asr",
            PipelineStage.Anchors => "anchors",
            PipelineStage.Transcript => "transcript",
            PipelineStage.Hydrate => "hydrate",
            _ => "mfa"
        };

    private static T AddErrorAndReturnDefault<T>(ICollection<string> errors, string message, T fallback)
    {
        errors.Add(message);
        return fallback;
    }
}

public sealed record OperatorControlOption(string Value, string Label);
