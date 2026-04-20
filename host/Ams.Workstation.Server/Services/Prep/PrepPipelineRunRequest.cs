using Ams.Core.Application.Commands;
using Ams.Core.Application.Mfa.Models;
using Ams.Core.Application.Pipeline;
using Ams.Core.Asr;

namespace Ams.Workstation.Server.Services.Prep;

public sealed record PrepPipelineRunRequest
{
    public static PrepPipelineRunRequest Default { get; } = new();

    public PipelineStage EndStage { get; init; } = PipelineStage.Mfa;
    public bool Force { get; init; }
    public bool ForceIndex { get; init; }
    public PrepPipelineAsrRequest Asr { get; init; } = PrepPipelineAsrRequest.Default;
    public PrepPipelineMfaRequest Mfa { get; init; } = PrepPipelineMfaRequest.Default;
    public PrepPipelineChunkRequest Chunking { get; init; } = PrepPipelineChunkRequest.Default;
}

public sealed record PrepPipelineAsrRequest
{
    public static PrepPipelineAsrRequest Default { get; } = new();

    public AsrEngine? Engine { get; init; }
    public string? Model { get; init; }
    public string? Language { get; init; } = GenerateTranscriptOptions.Default.Language;
    public bool EnableWordTimestamps { get; init; } = GenerateTranscriptOptions.Default.EnableWordTimestamps;
    public bool EnableFlashAttention { get; init; }
    public bool EnableDtwTimestamps { get; init; }
    public bool DisablePrompt { get; init; }
}

public sealed record PrepPipelineMfaRequest
{
    public static PrepPipelineMfaRequest Default { get; } = new();

    public MfaBeamProfile? BeamProfile { get; init; }
    public int? Beam { get; init; }
    public int? RetryBeam { get; init; }
}

public sealed record PrepPipelineChunkRequest
{
    public static PrepPipelineChunkRequest Default { get; } = new();

    public bool DisableChunkPlan { get; init; }
    public bool DisableChunkedMfa { get; init; }
    public bool RequireAsrChunkAudio { get; init; }
    public PrepPipelineChunkPolicyRequest Policy { get; init; } = PrepPipelineChunkPolicyRequest.Default;
}

public sealed record PrepPipelineChunkPolicyRequest
{
    public static PrepPipelineChunkPolicyRequest Default { get; } = new();

    public double? SilenceThresholdDb { get; init; }
    public double? MinSilenceDurationMs { get; init; }
    public double? MinChunkDurationSec { get; init; }
    public double? MaxChunkDurationSec { get; init; }
}
