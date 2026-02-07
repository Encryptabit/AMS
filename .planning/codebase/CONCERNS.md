# Codebase Concerns

**Analysis Date:** 2026-02-06

## Tech Debt

**Silent Exception Swallowing in Chapter Discovery:**
- Issue: Catches and silently swallows all exceptions during WAV file discovery, returning empty array with no logging
- File: `host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs`
- Impact: Masks real filesystem errors, makes debugging difficult
- Fix approach: Log exception at Warning level before returning empty array

**Inconsistent ASR Service URLs:**
- Issue: Default ASR URL hardcoded as `http://localhost:8000` in some places and `http://127.0.0.1:5000` in others
- Files: `host/Ams.Core/Asr/AsrClient.cs`, `host/Ams.Cli/Commands/AsrCommand.cs`, `host/Ams.Cli/Commands/PipelineCommand.cs`, `host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs`
- Impact: Confusion about which URL is correct; potential connection failures
- Fix approach: Centralize default URL in a single configuration constant or config file

**Hardcoded Batch Directory Name:**
- Issue: `DefaultBatchFolderName = "Batch 2"` hardcoded in pipeline command
- File: `host/Ams.Cli/Commands/PipelineCommand.cs`
- Impact: Users must use specific folder naming or always specify explicitly
- Fix approach: Make configurable via options or remove default

**Inefficient Audio Buffer Concatenation:**
- Issue: Uses manual `Array.Copy` in loop instead of `Buffer.BlockCopy` or `Span<T>` operations
- File: `host/Ams.Core/Artifacts/AudioBuffer.cs`
- Impact: Slow for large audio files (hours of content). Active TODO in code.
- Fix approach: Use `Buffer.BlockCopy` or `Span<T>` memory operations

**HttpClient Anti-Pattern:**
- Issue: Creates new `HttpClient` per `AsrClient` instead of using `IHttpClientFactory` or shared instance
- File: `host/Ams.Core/Asr/AsrClient.cs`
- Impact: Defeats connection pooling, creates socket pressure under load
- Fix approach: Use `IHttpClientFactory` registered in DI

**FFmpeg Codec Config Stub:**
- Issue: `avcodec_get_supported_config` stub returns 0 instead of actual value
- File: `host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`
- Impact: May cause issues when runtime negotiation is needed
- Fix approach: Implement proper codec config query (noted as TODO)

## Known Bugs

**No confirmed bugs from static analysis.**
- Race condition risk exists in process supervisors (see Fragile Areas)
- Error pattern service performance (see Performance Bottlenecks)

## Security Considerations

**Path Traversal Risk in Audio Controller:**
- Risk: URL path decoded but only file existence checked, no path normalization
- File: `host/Ams.Workstation.Server/Controllers/AudioController.cs`
- Current mitigation: None visible
- Recommendations: Use `Path.GetFullPath` and verify path is within expected directory

**PowerShell Execution Policy Bypass:**
- Risk: MFA process runner spawns PowerShell with `-ExecutionPolicy ByPass`
- File: `host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs`
- Current mitigation: Scripts written to temp files (not user-controlled input)
- Recommendations: Validate script content before execution; ensure temp file cleanup on crash

**Null-Forgiving Operator Without Validation:**
- Risk: `Path.GetDirectoryName(path)!` could produce NRE if path is root
- File: `host/Ams.Cli/Services/DspConfigService.cs`
- Current mitigation: None
- Recommendations: Add null check before using result

## Performance Bottlenecks

**Error Pattern Aggregation:**
- Problem: Iterates all chapters and loads all hydrate files sequentially for UI display
- File: `host/Ams.Workstation.Server/Services/ErrorPatternService.cs`
- Measurement: Documented as "5-10 seconds" for 50+ chapters
- Cause: Sequential I/O, no caching, no pagination
- Improvement path: Add caching layer, parallelize chapter loading, implement pagination

**Audio Buffer Concatenation:**
- Problem: Manual array copy loop for buffer concatenation
- File: `host/Ams.Core/Artifacts/AudioBuffer.cs`
- Cause: Uses `Array.Copy` in loop instead of bulk memory operations
- Improvement path: Use `Buffer.BlockCopy`, `Span<T>`, or SIMD-optimized operations

## Fragile Areas

**Process Supervisor State Machine:**
- File: `host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`
- Why fragile: Multiple static fields protected by `SemaphoreSlim` and `object` lock with unclear coordination. `volatile SupervisorState _state` read/written without consistent synchronization.
- Common failures: Race condition between checking and setting state; background warmup task swallows exceptions
- Safe modification: Audit all state transitions; consider using a proper state machine pattern
- Test coverage: No tests for process supervisors

**MFA Command Serialization:**
- File: `host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`
- Why fragile: Single `SemaphoreSlim CommandGate` serializes all MFA commands, creating bottleneck. Background warmup runs without coordination.
- Common failures: Race condition if main thread calls `EnsureReadyAsync` immediately after `TriggerBackgroundWarmup`
- Safe modification: Add proper async coordination between warmup and command execution
- Test coverage: No tests

**FileStream Resource Management:**
- File: `host/Ams.Workstation.Server/Controllers/AudioController.cs`
- Why fragile: FileStream created without guaranteed disposal if response streaming fails. TOCTOU gap between existence check and stream creation.
- Safe modification: Use `PhysicalFileResult` or ensure proper disposal chain
- Test coverage: No controller tests

## Scaling Limits

**Single-User Workspace:**
- File: `host/Ams.Workstation.Server/Services/BlazorWorkspace.cs`
- Current capacity: Single user (singleton workspace)
- Limit: Cannot support multiple concurrent users
- Symptoms at limit: State corruption, file access conflicts
- Scaling path: Would need per-session workspace or multi-tenant architecture

## Dependencies at Risk

**System.CommandLine Beta:**
- Package: System.CommandLine 2.0.0-beta4.22272.1 (`host/Ams.Cli/Ams.Cli.csproj`)
- Risk: Pre-release package; API may change or be discontinued
- Impact: CLI argument parsing would need rewrite
- Migration plan: Monitor for stable release; consider alternative (Spectre.Console.Cli)

**Whisper.NET CUDA Runtime:**
- Package: Whisper.NET.Runtime.Cuda 1.8.1
- Risk: CUDA version pinning; may not work with newer GPU drivers
- Impact: GPU acceleration unavailable if driver incompatible
- Migration plan: Keep updated with Whisper.NET releases

## Missing Critical Features

**No CI/CD Pipeline:**
- Problem: No automated build, test, or deployment pipeline detected
- Current workaround: Manual builds and testing
- Blocks: Cannot catch regressions automatically
- Implementation complexity: Low (add GitHub Actions workflow)

## Test Coverage Gaps

**Blazor Web UI (Zero Tests):**
- What's not tested: All Blazor pages, controllers, web services
- Files: `host/Ams.Workstation.Server/` (entire project)
- Risk: UI regressions, API contract breaks go unnoticed
- Priority: Medium
- Difficulty: Need to set up Blazor test infrastructure (bUnit)

**Process Supervisors (Zero Tests):**
- What's not tested: Process lifecycle, state transitions, race conditions, cleanup
- Files: `host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`, `host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`
- Risk: Concurrency bugs, resource leaks, silent failures
- Priority: High
- Difficulty: Need to mock process creation and async coordination

**MFA Workflow (Zero Tests):**
- What's not tested: File I/O error handling, corpus validation, G2P integration
- Files: `host/Ams.Core/Application/Mfa/MfaWorkflow.cs`, `host/Ams.Core/Application/Mfa/MfaService.cs`
- Risk: Pipeline failures in production scenarios
- Priority: Medium
- Difficulty: Need to mock external MFA tool interactions

**ASR Client HTTP (Zero Tests):**
- What's not tested: HTTP retry logic, timeout handling, error responses
- File: `host/Ams.Core/Asr/AsrClient.cs`
- Risk: Silent failures on network issues
- Priority: Medium
- Difficulty: Low (mock HttpClient or use test server)

---

*Concerns audit: 2026-02-06*
*Update as issues are fixed or new ones discovered*
