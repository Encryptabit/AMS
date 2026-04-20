using Ams.Core.Application.Commands;
using Ams.Core.Application.Mfa.Models;
using Ams.Core.Application.Pipeline;
using Ams.Core.Asr;
using Ams.Workstation.Server.Components.Navigation;
using Ams.Workstation.Server.Services.Prep;

namespace Ams.Tests.Workstation.Prep;

public sealed class WorkstationPrepOperatorControlTests
{
    [Fact]
    public void DefaultControls_BuildDeterministicRequest()
    {
        var controls = new PrepPipelineOperatorControls();

        var success = controls.TryBuildRequest(out var request);

        Assert.True(success);
        Assert.Equal(PipelineStage.Mfa, request.EndStage);
        Assert.False(request.Force);
        Assert.False(request.ForceIndex);
        Assert.Null(request.Asr.Engine);
        Assert.Equal(GenerateTranscriptOptions.Default.Language, request.Asr.Language);
        Assert.Equal(PrepPipelineRunRequest.Default.Asr.EnableWordTimestamps, request.Asr.EnableWordTimestamps);
        Assert.Null(request.Mfa.BeamProfile);
        Assert.Null(request.Mfa.Beam);
        Assert.Null(request.Mfa.RetryBeam);
        Assert.False(request.Chunking.DisableChunkPlan);
        Assert.False(request.Chunking.DisableChunkedMfa);
        Assert.False(request.Chunking.RequireAsrChunkAudio);
        Assert.Null(controls.LastValidationMessage);
        Assert.Null(controls.LastNormalizationWarning);
    }

    [Fact]
    public void GetValidationMessage_InvalidSelectionAndMalformedNumbers_ReturnsActionableMessage()
    {
        var controls = new PrepPipelineOperatorControls();

        var missingChapterMessage = controls.GetValidationMessage("Missing Chapter", ["Chapter 1"]);

        Assert.NotNull(missingChapterMessage);
        Assert.Contains("no longer available", missingChapterMessage, StringComparison.OrdinalIgnoreCase);

        controls.MfaRetryBeamText = "abc";
        var malformedValueMessage = controls.GetValidationMessage("Chapter 1", ["Chapter 1"]);

        Assert.NotNull(malformedValueMessage);
        Assert.Contains("MFA retry beam", malformedValueMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("whole number", malformedValueMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetCompatibilityWarnings_WhisperXRefinementAndChunkConflicts_AreExplicit()
    {
        var controls = new PrepPipelineOperatorControls
        {
            AsrEngineKey = PrepPipelineOperatorControls.EngineWhisperXKey,
            EnableDtwTimestamps = true,
            EnableFlashAttention = true,
            DisableChunkedMfa = true,
            RequireAsrChunkAudio = true
        };

        var warnings = controls.GetCompatibilityWarnings();

        Assert.Equal(3, warnings.Count);
        Assert.Contains(warnings, warning => warning.Contains("DTW", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(warnings, warning => warning.Contains("FlashAttention", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(warnings, warning => warning.Contains("chunk audio", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TryBuildRequest_MapsControlValuesToTypedRequest()
    {
        var controls = new PrepPipelineOperatorControls
        {
            EndStageKey = "hydrate",
            Force = true,
            ForceIndex = true,
            AsrEngineKey = PrepPipelineOperatorControls.EngineWhisperKey,
            AsrModel = "custom-model",
            AsrLanguage = "fr",
            EnableWordTimestamps = false,
            EnableFlashAttention = true,
            EnableDtwTimestamps = true,
            DisablePrompt = true,
            MfaBeamProfileKey = PrepPipelineOperatorControls.BeamProfileStrictKey,
            MfaBeamText = "64",
            MfaRetryBeamText = "192",
            DisableChunkPlan = true,
            DisableChunkedMfa = false,
            RequireAsrChunkAudio = true,
            ChunkSilenceThresholdDbText = "-40.5",
            ChunkMinSilenceDurationMsText = "260",
            ChunkMinDurationSecText = "2.5",
            ChunkMaxDurationSecText = "7.5"
        };

        var success = controls.TryBuildRequest(out var request);

        Assert.True(success);
        Assert.Equal(PipelineStage.Hydrate, request.EndStage);
        Assert.True(request.Force);
        Assert.True(request.ForceIndex);

        Assert.Equal(AsrEngine.Whisper, request.Asr.Engine);
        Assert.Equal("custom-model", request.Asr.Model);
        Assert.Equal("fr", request.Asr.Language);
        Assert.False(request.Asr.EnableWordTimestamps);
        Assert.True(request.Asr.EnableFlashAttention);
        Assert.True(request.Asr.EnableDtwTimestamps);
        Assert.True(request.Asr.DisablePrompt);

        Assert.Equal(MfaBeamProfile.Strict, request.Mfa.BeamProfile);
        Assert.Equal(64, request.Mfa.Beam);
        Assert.Equal(192, request.Mfa.RetryBeam);

        Assert.True(request.Chunking.DisableChunkPlan);
        Assert.False(request.Chunking.DisableChunkedMfa);
        Assert.True(request.Chunking.RequireAsrChunkAudio);
        Assert.Equal(-40.5, request.Chunking.Policy.SilenceThresholdDb);
        Assert.Equal(260, request.Chunking.Policy.MinSilenceDurationMs);
        Assert.Equal(2.5, request.Chunking.Policy.MinChunkDurationSec);
        Assert.Equal(7.5, request.Chunking.Policy.MaxChunkDurationSec);
    }

    [Fact]
    public void TryBuildRequest_MalformedValue_RevertsToLastKnownValidSelection()
    {
        var controls = new PrepPipelineOperatorControls
        {
            EndStageKey = "anchors",
            MfaBeamText = "12"
        };

        var firstSuccess = controls.TryBuildRequest(out var validRequest);

        Assert.True(firstSuccess);
        Assert.Equal("12", controls.MfaBeamText);

        controls.MfaBeamText = "twelve";

        var secondSuccess = controls.TryBuildRequest(out var fallbackRequest);

        Assert.False(secondSuccess);
        Assert.Equal(validRequest, fallbackRequest);
        Assert.Equal("12", controls.MfaBeamText);
        Assert.NotNull(controls.LastNormalizationWarning);
        Assert.Contains("reverted", controls.LastNormalizationWarning, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyRequest_RoundTripsTypedSnapshotValues()
    {
        var expected = new PrepPipelineRunRequest
        {
            EndStage = PipelineStage.Transcript,
            Force = true,
            ForceIndex = true,
            Asr = new PrepPipelineAsrRequest
            {
                Engine = AsrEngine.WhisperX,
                Model = "whisperx-large-v3",
                Language = "es",
                EnableWordTimestamps = false,
                EnableFlashAttention = true,
                EnableDtwTimestamps = true,
                DisablePrompt = true
            },
            Mfa = new PrepPipelineMfaRequest
            {
                BeamProfile = MfaBeamProfile.Fast,
                Beam = 48,
                RetryBeam = 96
            },
            Chunking = new PrepPipelineChunkRequest
            {
                DisableChunkPlan = false,
                DisableChunkedMfa = true,
                RequireAsrChunkAudio = false,
                Policy = new PrepPipelineChunkPolicyRequest
                {
                    SilenceThresholdDb = -36.5,
                    MinSilenceDurationMs = 320,
                    MinChunkDurationSec = 1.75,
                    MaxChunkDurationSec = 8.5
                }
            }
        };

        var controls = new PrepPipelineOperatorControls();
        controls.ApplyRequest(expected);

        var success = controls.TryBuildRequest(out var rebuilt);

        Assert.True(success);
        Assert.Equal(expected, rebuilt);
    }

    [Fact]
    public void TryBuildRequest_MalformedValue_SurfacesNormalizationWarningInCompatibilityList()
    {
        var controls = new PrepPipelineOperatorControls
        {
            EndStageKey = "anchors",
            MfaBeamText = "12"
        };

        Assert.True(controls.TryBuildRequest(out _));

        controls.MfaBeamText = "not-a-number";

        Assert.False(controls.TryBuildRequest(out _));

        var warnings = controls.GetCompatibilityWarnings();

        Assert.Contains(warnings, warning => warning.Contains("reverted", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(warnings, warning => warning.Contains("MFA beam", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("/prep", true)]
    [InlineData("/prep/pipeline", false)]
    public void PrepRouteAliases_ResolveToPipelineModule(string path, bool expectedAlias)
    {
        var match = StageRouteCatalog.Resolve(path);

        Assert.NotNull(match);
        Assert.Equal(StageRouteCatalog.StageIds.Prep, match!.Stage.Id);
        Assert.Equal(StageRouteCatalog.ModuleIds.PrepPipeline, match.Module.Id);
        Assert.Equal(expectedAlias, match.IsCompatibilityAlias);
    }
}
