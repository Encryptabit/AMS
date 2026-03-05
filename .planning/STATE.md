# AMS Project State

## Brief Summary

Audio Management System - CLI and core library for audio processing, ASR, forced alignment, and audiobook mastering. v2.0 pivoted from native desktop UI to Blazor workstation.

## Current Position

**Milestone**: v2.0 Blazor Workstation
**Phase**: 14 - Shared Chunked ASR/MFA
**Plan**: 7/7
**Status**: Phase 14 complete; rollout controls (--no-chunk-plan, --no-chunked-mfa) and verification checklist added

## Progress

```
v1.0 Codebase Audit    [████████████████████] 100% - SHIPPED
v1.1 Execute Refactor  [████████████████████] 100% - SHIPPED
v2.0 Blazor Workstation[████████████████████] 100% - Phase 10 complete
```

## Phase 14 Plans (Shared Chunked ASR/MFA)

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 14-01 | Chunk Plan Artifact Model | 2 | Complete |
| 14-02 | Chunk Planning Service | 2 | Complete |
| 14-03 | ASR Chunk-Plan Integration | 2 | Complete |
| 14-04 | Chunked MFA Corpus Builder | 2 | Complete |
| 14-05 | TextGrid Chunk Aggregation | 2 | Complete |
| 14-06 | MFA Beam Profiles & Adaptive Retry | 2 | Complete |
| 14-07 | Rollout Controls & Verification | 2 | Complete |

## Phase 13 Plans (Pickup Substitution)

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 13-01 | Audio Infrastructure (24-bit, Roomtone Helpers) | 2 | Complete |
| 13-02 | Cross-Chapter Models & Mini Waveform | 2 | Complete |
| 13-03 | Pickup Substitution Page & PickupBox | 2 | Complete |
| 13-04 | Upfront Processing & Stage/Unstage Actions | 2 | Complete |
| 13-05 | Region Editing & Commit/Revert Flow | 2 | Complete |
| 13-06 | Roomtone Operations | 2 | Complete |
| 13-07 | Integration & Cleanup | 2 | Complete |
| 13-08 | Integration & Verification | - | Pending |

## Phase 12.1 Plans (MFA Refinement for Pickup Timings)

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 12.1-01 | MFA Refinement for Pickup Timings | 2 | Complete |

## Phase 12 Plans (Polish Area Foundation)

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 12-01 | Audio Splice Service & Domain Models | 2 | Complete |
| 12-02 | Staging Queue & Undo Services | 2 | Complete |
| 12-03 | Pickup Matching & Polish Orchestration | 2 | Complete |
| 12-04 | Waveform Region Editing | 2 | Complete |
| 12-05 | Polish Page Layout | 3 | Complete |
| 12-06 | Multi-Waveform & Batch Operations | 2 | Complete |
| 12-07 | Post-Replacement Verification | 2 | Complete |
| 12-08 | Result Verification | - | Pending |

## Phase 10 Plans

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 10-01 | Proof Backend Services | 3 | Complete |
| 10-02 | Book Overview Page | 4 | Complete |
| 10-03 | Error Patterns Aggregation | 3 | Complete |
| 10-04 | Errors View Enhancement | 3 | Complete |
| 10-05 | Review Status & Ignored Patterns | 6 | Complete |
| 10-06 | Audio Export & CRX Foundation | 3 | Complete |

## Phase 10.2 Plans (CRX Excel & API Cleanup - Complete)

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 10.2-01 | CRX Excel Integration & API Cleanup | 2 | Complete |

## Phase 10.1 Plans (INSERTED - Complete)

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 10.1-01 | AudioTreatmentService + TreatCommand | 3 | Complete |

## Phase 9 Plans

| Plan | Name | Tasks | Status |
|------|------|-------|--------|
| 09-01 | Project Foundation & DI | 3 | Complete |
| 09-02 | Ams.Core Integration & Layout Shell | 6 | Complete |
| 09-03 | Waveform Component & JS Interop | 4 | Complete |
| 09-04 | Real Data Integration (Audio + Sentences) | 4 | Complete |
| 09-05 | Chapter Discovery Consolidation | 3 | Complete |
| 09-06 | Layout Lockdown (BitGrid) | 5 | Deferred |

## Future Objectives (Unscheduled)

- Keyboard Navigation & SentenceList Component enhancements
- Switchable alignment mode (DTW chunked vs MFA) for workflow-specific precision/speed tradeoffs
  - Plan: `.planning/quick/002-switchable-dtw-mfa-alignment-mode/002-PLAN.md`

## Key Decisions (v2.0)

| Decision | Choice | Rationale |
|----------|--------|-----------|
| VelloSharp + Avalonia | NO-GO | Child window surface creation fails |
| VelloSharp + WPF | NO-GO | VelloView composition fails |
| VelloSharp + WinUI | NO-GO | Exit code 22 |
| VelloSharp + pure Winit | WORKS | Standalone window renders perfectly |
| Hybrid WPF+Vello | CONFIRMED GO | Owned window bypasses wgpu child limitation |
| **Desktop UI v2.0** | **ON ICE** | Complexity too high for current needs |
| **Blazor Server** | **CHOSEN** | Direct Ams.Core integration, wavesurfer.js for audio |
| book-index sections | sections[].title | Chapter list from sections array, not chapters |
| State persistence | LocalAppData | %LOCALAPPDATA%\AMS\workstation-state.json |
| Diff pipeline stabilization | Deterministic op-order + strict sentence ownership | Remove synthetic diff artifacts (hybrids/reorder/boundary bleed) in reviewer workflows |
| Persistence services | Singleton + LocalAppData JSON | ReviewedStatusService and IgnoredPatternsService persist per-book state |
| AggregatePatterns param type | IReadOnlySet<string> | Compatible with IgnoredPatternsService.GetIgnoredKeys() return type |
| Audio segment export | AudioProcessor.Trim + ToWavStream | FFmpeg atrim for segment extraction, sequential WAV numbering in CRX folder |
| CRX tracking format | Excel (.xlsx) via ClosedXML | Matches Python validation-viewer column layout for publisher submission |
| AudioSpliceService pattern | Static/stateless like AudioProcessor | FFmpeg acrossfade for crossfaded splicing, no DI needed |
| Crossfade clamping | 30% of shorter segment | Prevents boundary overflow per research Pitfall 2 |
| Region callback naming | OnRegionBoundsUpdated JSInvokable | Distinct from OnRegionUpdated EventCallback to avoid C# ambiguity |
| Region update event | update-end (not update) | Avoids continuous drag event overhead on Blazor circuit |
| Polish services persistence | Workspace-local .polish/ dirs | StagingQueue and UndoService persist to workspace, not AppData |
| Undo versioning | Versioned segment files + manifest.json | sent{id}.v{N}.original.wav with per-chapter JSON manifest |
| ASR model path for Polish | AsrEngineConfig.ResolveModelPath | Uses AMS_WHISPER_MODEL_PATH env var, consistent with CLI pattern |
| Pickup text normalization | Inline simple normalization | Avoids contraction expansion that would reduce ASR match fidelity |
| Polish page two-column layout | BitGrid 7/5 split | Importer (left) + staging queue (right) for ergonomic Polish workflow |
| CRX counts on Polish landing | CrxService.GetEntries() filtered | Per-chapter error count from shared CRX Excel data |
| Flagged sentence selection | error status + diff ops | Sentences eligible for pickup replacement have errors or non-empty diffs |
| Region endpoint decode | AudioProcessor.Decode with start/duration | Partial disk decode for memory-efficient multi-chapter loading |
| syncPlayheads per-waveform | Individual SeekTo calls via JS interop | Each waveform may have different region offsets, so sync individually |
| BatchOperationService lifetime | Transient | Batch history in-memory, operations staged through StagingQueueService |
| DSP tab | Placeholder only | Deferred per locked architectural decision |
| Revalidation pass threshold | 0.9 Levenshtein similarity | High enough for quality, allows minor ASR variance |
| Revalidation history | In-memory per chapter | Ephemeral verification state, cleared on chapter switch |
| SyncToProof granularity | Per-chapter via ReviewedStatusService | Aligns with existing Proof area chapter-level tracking |
| InternalsVisibleTo for Workstation | Assembly attribute on Ams.Core | MfaService/MfaProcessSupervisor are internal; workstation is first-party host |
| MFA warmup at workstation startup | TriggerBackgroundWarmup in Program.cs | Pre-warm conda environment to avoid first-use latency on pickup import |
| MFA cache key scope | Audio identity + sentence IDs + normalized BookText | Prevents stale MFA timings when same audio is re-matched with different sentences |
| Cross-chapter composite key | chapterStem:sentenceId | Prevents sentence ID collisions when processing pickups across all chapters |
| Mini waveform rendering | Canvas-based drawMiniWaveform | Lightweight alternative to wavesurfer instances for match box thumbnails |
| Waveform amplitude API | RMS per block, normalized 0-1 | Server-side computation, clamped 20-500 points for safety |
| 24-bit WAV encoding | PCM_S24LE codec + S32 input format | FFmpeg standard pattern; codec truncates 32-bit to 24-bit in WAV container |
| Bit depth detection | bits_per_raw_sample > bits_per_coded_sample > format inference | Multi-level fallback covers PCM, compressed, and edge cases |
| Roomtone fill implementation | Sample-level Array.Copy loop | More efficient than FFmpeg filter graph for simple memory looping |
| Pickup substitution route | /polish (finalized in 13-07) | Index.razor deleted; PickupSubstitution.razor now owns /polish directly |
| Roomtone undo sentenceId | -1 sentinel | Roomtone ops are not sentence-specific; undo tracks by replacement ID |
| Roomtone region color | Blue rgba(59,130,246,0.3) | Distinct from green (staged) and gray (committed) regions |
| PickupBox mini waveform lifecycle | Import + draw + dispose per render | Avoids long-lived JS module refs for many PickupBox instances |
| Region sync strategy | Clear-all + re-add on state change | Guaranteed consistency vs incremental add/remove; minor perf tradeoff |
| Completion auto-advance | Wrap-around search with 500ms delay | Searches forward then wraps; delay provides visual feedback before flip |
| Old Polish page decommission | Delete entirely (Index.razor, ChapterPolish.razor) | PickupSubstitution replaces them per locked decision; shared components also deleted |
| Audio deallocation on chapter flip | Deallocate corrected buffer via CurrentChapterHandle | Prevents memory pressure during multi-chapter navigation |
| Chunk plan artifact naming | {chapterStem}.align.chunks.json | Follows existing align.tx/hydrate/anchors convention |
| ChunkPlanPolicy fields | threshold, min silence, min chunk, sample rate | Enables downstream plan validity checking |
| ChunkPlanEntry dual fields | Sample-precise + time-domain | StartSample/LengthSamples for slicing, StartSec/EndSec for offsets |
| ChunkPlanningPolicy vs ChunkPlanPolicy | Separate input/persisted types | Service resolves defaults; persisted record stores exact values used |
| Audio fingerprint strategy | path+length+sampleRate+channels | Lightweight identity for invalidation; avoids content hashing overhead |
| Path separator normalization | Backslash to forward slash in fingerprint | Cross-platform consistency between Windows and Linux |
| ASR chunk-plan sourcing | ChunkPlanningService.GeneratePlan + chapter.Documents.ChunkPlan | ASR generates/persists plan when missing/stale, reuses when valid |
| Monotonic merge timestamps | High-water-mark clamping in MergeChunkResponses | Prevents boundary overlap regression in merged ASR output |
| Merge chunk ordering | Sort by OffsetSec before merge | Deterministic token/segment ordering regardless of input order |
| MergeChunkResponses visibility | internal (was private) | Enables direct unit testing without integration complexity |
| Chunk corpus activation threshold | >1 chunks in plan | Single-chunk plans use legacy single-utterance path to avoid overhead |
| Chunked MFA fallback strategy | Omit ASR corpus retry | ASR corpus fallback only applies to legacy single-utterance lab failures |
| TextGrid aggregation format | Standard Praat full-text | Compatible with existing TextGridParser; includes words+phones tiers |
| Aggregation source directory | mfaCopyDir (post-collection) | Consistent file resolution after per-chunk TextGrids are collected |
| Interval ordering strategy | Sort by start time after offset | Guaranteed monotonic ordering for downstream MergeTimingsCommand |
| MinLabTokenCount | 2 | Prevents empty/trivial lab files that cause MFA alignment errors |
| Nearest-sentence fallback | Timing midpoint proximity | Deterministic fallback for chunks with no direct sentence timing overlap |
| MFA beam profile defaults | Fast=20/80, Balanced=40/120, Strict=80/200 | Configurable presets with explicit override precedence |
| Coverage heuristic threshold | 0.15 ratio (words / expected at 3.3 words/sec) | Low enough to catch real failures, avoids false positives on natural speech variation |
| Adaptive retry strategy | Re-align full corpus with strict beam, collect only failed chunks | Simpler than subset corpus; MFA handles utterance-level recovery internally |
| Rollout flag defaults | Both flags false (chunking enabled) | New behavior is default; flags revert to legacy without code changes |
| DisableChunkPlan scope | Skips chunk plan in AsrService + MFA has no plan to use | Single flag reverts both stages to legacy behavior |
| DisableChunkedMfa scope | MFA-only; ASR chunking unaffected | Allows isolating MFA chunking behavior independently |

## Phase 8/8.1 Conclusions (Archived)

<details>
<summary>Native Desktop UI POC Results</summary>

**Decision: Defer desktop UI, pursue Blazor workstation instead**

Key findings from POC work:
1. **SkiaSharp GPU (SKGLElement)**: OpenGL context creation fails on WPF
2. **SkiaSharp CPU (SKElement)**: Works but not GPU-accelerated
3. **VelloSharp**: Requires complex owned-window architecture due to wgpu limitations
4. **Both**: Required mipmap infrastructure for acceptable waveform performance
5. **Blazor + wavesurfer.js**: Battle-tested solution, works immediately

The effort to build a native desktop waveform viewer exceeds the value for current validation needs.

### POC Created

```
poc/HybridVelloPoc/    - WPF + owned Vello window (validated)
poc/SkiaSharpPoc/      - WPF + SkiaSharp (CPU only worked)
poc/VelloSharpPoc/     - Avalonia + VelloSharp (child window fails)
```

</details>

## Accumulated Context

### Roadmap Evolution

- Phase 13 added: Pickup Substitution
- Phase 12.1 inserted after Phase 12: MFA refinement for pickup timings — phoneme-accurate pickup boundaries via forced alignment (URGENT)

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 3 | frequency dict + proper noun extraction | 2026-03-02 | ed4091f | [3-english-frequency-dictionary-infrastruct](./quick/3-english-frequency-dictionary-infrastruct/) |
| 4 | wire .WithPrompt in AsrProcessor | 2026-03-02 | d5372fe | [4-wire-withprompt-in-asrprocessor](./quick/4-wire-withprompt-in-asrprocessor/) |
| 5 | AudioBuffer contiguous backing + Memory<float> slicing | 2026-03-02 | 7ddc99c | [5-audiobuffer-contiguous-backing-memory-slice](./quick/5-audiobuffer-contiguous-backing-memory-slice/) |
| 6 | silence-based pre-chunking in AsrProcessor | 2026-03-02 | f76a5c3 | [6-silence-based-pre-chunking-in-asrprocess](./quick/6-silence-based-pre-chunking-in-asrprocess/) |
| 8 | audiobook QC CLI command (ffmpeg silencedetect) | 2026-03-02 | a11fd4b | [8-add-audiobook-qc-cli-command-ffmpeg-base](./quick/8-add-audiobook-qc-cli-command-ffmpeg-base/) |

## Next Action

Phase 14 complete. All 7 plans executed. Populate 14-VERIFICATION.md with runtime benchmarks and complete sign-off checklist before promoting shared chunking as default.

## Deferred UI Refinements (for Plan 10-04)

- Selection flash: Active sentence should briefly flash lighter shade when selected
- Conditional left border: Only show colored border for sentences with errors

## Deferred Issues

None currently.

## Removed Projects

- `host/Ams.Web*` - Removed in favor of Ams.Workstation.Server (Blazor Server approach)

## Session Continuity

Last activity: 2026-03-05 - Completed plan 14-07: Rollout controls and phase verification checklist (Phase 14 complete)
