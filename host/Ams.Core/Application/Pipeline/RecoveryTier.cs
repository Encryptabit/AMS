namespace Ams.Core.Application.Pipeline;

// Pipeline orchestration tier for the ASR-recovery state machine. The first attempt runs at
// None (user's flags as-is). When MFA reports low-coverage chunks, the orchestrator escalates:
// AlternateModel re-runs ASR with the cross-pair Whisper model (prompt preserved); Promptless
// is the last resort (original model, prompt suppressed). MfaWorkflow doesn't track tiers —
// it just reports the boolean retry signal — so this enum lives at the orchestrator layer.
public enum RecoveryTier
{
    None,
    AlternateModel,
    Promptless
}
