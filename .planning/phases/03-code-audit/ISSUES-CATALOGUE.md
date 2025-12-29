# AMS Code Audit Issues Catalogue

**Analysis Date:** 2025-12-28
**Source Documents:**
- DEAD-CODE.md - Dead code analysis
- RESPONSIBILITY-MAP.md - Architecture and responsibility analysis
- PROJECT-AUDIT.md - Non-active project verification

---

## Summary Statistics

| Category | Critical | High | Medium | Low | Total |
|----------|----------|------|--------|-----|-------|
| Dead Code | 0 | 2 | 5 | 3 | 10 |
| Scattered Logic | 0 | 2 | 2 | 1 | 5 |
| Over-Abstraction | 0 | 1 | 4 | 0 | 5 |
| Project Status | 0 | 1 | 2 | 3 | 6 |
| Technical Debt | 0 | 1 | 2 | 2 | 5 |
| **Total** | **0** | **7** | **15** | **9** | **31** |

**Key Insight:** No critical issues identified. Seven high-priority items warrant immediate attention in Phase 4.

---

## Issues by Priority

### High Priority (7 issues)

| ID | Category | Description | Effort | Dependencies |
|----|----------|-------------|--------|--------------|
| AUD-001 | Dead Code | DspDemoRunner.cs - 141 lines of unused demo code | Small | None |
| AUD-002 | Dead Code | SentenceTimelineBuilder.cs - 182 lines, zero callers | Small | None |
| AUD-003 | Scattered Logic | AlignmentService god class - 681 lines, 4 responsibilities | Large | None |
| AUD-004 | Scattered Logic | Section resolution duplicated in ChapterContext and AlignmentService | Medium | None |
| AUD-005 | Over-Abstraction | IAudioService interface and empty AudioService implementation | Small | None |
| AUD-006 | Project Status | OverlayTest broken - references removed method | Small | None |
| AUD-007 | Technical Debt | AudioProcessorFilterTests failing - FFmpeg filter syntax | Medium | None |

### Medium Priority (15 issues)

| ID | Category | Description | Effort | Dependencies |
|----|----------|-------------|--------|--------------|
| AUD-008 | Dead Code | 4 empty Whisper.NET placeholders (Wn*.cs) - 32 lines | Small | None |
| AUD-009 | Dead Code | ManifestV2.cs - 19 lines, superseded format | Small | None |
| AUD-010 | Dead Code | Class1.cs in Ams.Web.Shared - template artifact | Small | None |
| AUD-011 | Dead Code | 9 unused AudioProcessor methods - ~185 lines | Medium | Verify no callers |
| AUD-012 | Dead Code | 2 duplicate methods in AlignmentService | Small | AUD-004 |
| AUD-013 | Scattered Logic | ASR buffer preparation split between two locations | Medium | None |
| AUD-014 | Scattered Logic | MFA artifacts in wrong location (should be Application/Mfa) | Small | None |
| AUD-015 | Over-Abstraction | IMfaService - single implementation, unused via DI | Small | Verify usage |
| AUD-016 | Over-Abstraction | IBookParser - single implementation, questionable value | Small | Verify usage |
| AUD-017 | Over-Abstraction | IBookIndexer - single implementation, questionable value | Small | Verify usage |
| AUD-018 | Over-Abstraction | IBookCache - single implementation, questionable value | Small | Verify usage |
| AUD-019 | Project Status | Ams.UI.Avalonia dormant - empty skeleton | Small | None |
| AUD-020 | Project Status | Ams.Tests 2 failing tests need fix | Medium | AUD-007 |
| AUD-021 | Technical Debt | DateTime warning in Validation.razor | Small | None |
| AUD-022 | Technical Debt | Inconsistent document loading patterns across subsystems | Large | None |

### Low Priority (9 issues)

| ID | Category | Description | Effort | Dependencies |
|----|----------|-------------|--------|--------------|
| AUD-023 | Dead Code | InspectDocX analysis tool - one-time utility | Small | None |
| AUD-024 | Dead Code | Ams.UI.Avalonia skeleton - no functionality | Small | None |
| AUD-025 | Dead Code | Commented-out SnapToEnergyAuto reference | Small | None |
| AUD-026 | Scattered Logic | Validation files scattered across multiple locations | Medium | None |
| AUD-027 | Project Status | InspectDocX should be archived or removed | Small | None |
| AUD-028 | Project Status | Web stack needs auth for multi-user deployment | Large | None |
| AUD-029 | Project Status | Missing MFA integration tests | Large | None |
| AUD-030 | Technical Debt | Runtime subsystem oversized (28 files) | Large | AUD-003 |
| AUD-031 | Technical Debt | Prosody patterns need standardization | Medium | None |

---

## Issue Details

### AUD-001: DspDemoRunner.cs Unused Demo Code

**Category:** Dead Code
**Severity:** High
**Effort:** Small (1 hour)

**Description:**
`host/Ams.Core/Audio/DspDemoRunner.cs` contains 141 lines of demo code for exercising the native DSP wrapper. It has zero production callers.

**Impact:**
- Increases codebase size without value
- May confuse new developers about code purpose
- Maintenance burden during refactoring

**Recommendation:**
Delete file. Consider converting to a test in Ams.Tests if DSP wrapper regression testing is needed.

**Dependencies:** None

---

### AUD-002: SentenceTimelineBuilder.cs Zero Callers

**Category:** Dead Code
**Severity:** High
**Effort:** Small (1 hour)

**Description:**
`host/Ams.Core/Audio/SentenceTimelineBuilder.cs` contains 182 lines with zero callers for its `Build()` method and zero references to the `SentenceTimelineEntry` type.

**Impact:**
- 182 lines of dead code
- Likely superseded by `PauseMapBuilder` or similar
- Confusing presence in Audio namespace

**Recommendation:**
Delete file after confirming no hidden callers (e.g., via reflection).

**Dependencies:** None

---

### AUD-003: AlignmentService God Class

**Category:** Scattered Logic
**Severity:** High
**Effort:** Large (16-24 hours)

**Description:**
`AlignmentService` at 681 lines has 4 distinct responsibilities:
1. Anchor computation
2. Transcript indexing
3. Transcript hydration
4. Section resolution

This violates Single Responsibility Principle and makes testing difficult.

**Impact:**
- Difficult to unit test individual responsibilities
- High coupling within the class
- Changes to one responsibility risk breaking others
- Hard to understand and maintain

**Recommendation:**
Split into focused classes:
- `AnchorComputeService`
- `TranscriptIndexService`
- `TranscriptHydrationService`
- `ChapterLabelResolver` (utility, not service)

**Dependencies:** None (but would enable other refactorings)

---

### AUD-004: Section Resolution Duplication

**Category:** Scattered Logic
**Severity:** High
**Effort:** Medium (4-6 hours)

**Description:**
Section resolution logic is duplicated in both `ChapterContext` and `AlignmentService`. This creates:
- Two implementations to maintain
- Risk of divergent behavior
- Unclear ownership

**Impact:**
- Bug-prone duplication
- Confusion about which implementation to use
- Maintenance overhead

**Recommendation:**
Extract `ChapterLabelResolver` utility class. Have both `ChapterContext` and `AlignmentService` delegate to it.

**Dependencies:** None

---

### AUD-005: IAudioService Empty Placeholder

**Category:** Over-Abstraction
**Severity:** High
**Effort:** Small (30 minutes)

**Description:**
`IAudioService` interface and `AudioService` implementation exist but:
- `AudioService.WarmAsync()` does nothing (returns `Task.CompletedTask`)
- Zero consumers of the interface
- Never registered in DI container

**Impact:**
- Misleading presence suggests functionality that doesn't exist
- Maintenance burden during interface changes

**Recommendation:**
Delete both `IAudioService.cs` and `AudioService.cs`.

**Dependencies:** None

---

### AUD-006: OverlayTest Build Broken

**Category:** Project Status
**Severity:** High
**Effort:** Small (5 minutes)

**Description:**
`analysis/OverlayTest/Program.cs` references `AudioProcessor.OverlayRoomtone()` which no longer exists. The project fails to build.

**Impact:**
- Broken build if included in solution
- Indicates stale code
- Confuses developers about roomtone functionality

**Recommendation:**
Delete project directory and remove from solution. The roomtone functionality was intentionally removed during pipeline simplification.

**Dependencies:** None

---

### AUD-007: AudioProcessorFilterTests FFmpeg Failures

**Category:** Technical Debt
**Severity:** High
**Effort:** Medium (2-4 hours)

**Description:**
Two tests fail due to FFmpeg filter graph configuration issues:
- `Trim_ReturnsExpectedSegment` - fails with "Invalid argument"
- `FadeIn_SetsLeadingSamplesToZero` - fails with "Invalid argument"

The issue appears to be FFmpeg filter syntax (locale-dependent decimal separators or Windows-specific path issues).

**Impact:**
- 2 failing tests in test suite
- May indicate real issue with Trim/FadeIn on Windows
- Reduces confidence in audio processing

**Recommendation:**
Investigate FFmpeg filter syntax. Likely needs `CultureInfo.InvariantCulture` for decimal formatting in filter strings.

**Dependencies:** None

---

### AUD-008: Empty Whisper.NET Placeholders

**Category:** Dead Code
**Severity:** Medium
**Effort:** Small (15 minutes)

**Description:**
Four empty placeholder files in `Services/Integrations/ASR/WhisperNet/`:
- `WnModel.cs` (8 lines)
- `WnSession.cs` (8 lines)
- `WnTranscriber.cs` (8 lines)
- `WnUtils.cs` (8 lines)

These were scaffolded for a refactoring that never happened. Actual Whisper.NET integration lives in `AsrProcessor.cs`.

**Impact:**
- 32 lines of dead code
- Misleading project structure

**Recommendation:**
Delete all four files.

**Dependencies:** None

---

### AUD-009: ManifestV2.cs Superseded

**Category:** Dead Code
**Severity:** Medium
**Effort:** Small (15 minutes)

**Description:**
`ManifestV2.cs` (19 lines) is a superseded manifest format with zero references. The system now uses `ChapterContext` and document slot pattern.

**Impact:**
- Orphaned model class
- Confusing presence

**Recommendation:**
Delete file.

**Dependencies:** None

---

### AUD-010: Class1.cs Template Artifact

**Category:** Dead Code
**Severity:** Medium
**Effort:** Small (5 minutes)

**Description:**
`host/Ams.Web.Shared/Class1.cs` is an empty placeholder from project template generation.

**Impact:**
- 5 lines of dead code
- Unprofessional appearance

**Recommendation:**
Delete file.

**Dependencies:** None

---

### AUD-011: Unused AudioProcessor Methods

**Category:** Dead Code
**Severity:** Medium
**Effort:** Medium (2-3 hours)

**Description:**
9 methods in `AudioProcessor` and `AudioProcessor.Analysis` have zero callers:
- `AdjustVolume` (~15 lines)
- `FadeOut` (~15 lines)
- `EncodeWav` (~10 lines)
- `NormalizeLoudness` (~40 lines)
- `AnalyzeGap` (~30 lines)
- `FindSpeechEndFromGap` (~25 lines)
- `SnapToEnergyAuto` (~25 lines)
- Plus 2 duplicate methods in AlignmentService

Total: ~185 lines

**Impact:**
- Significant dead code in core audio processing
- Maintenance burden
- May have been useful features that were disabled

**Recommendation:**
Remove methods after final verification. Consider documenting why they were removed (may be useful for future reference).

**Dependencies:** Verify no dynamic/reflection calls

---

### AUD-012: Duplicate Methods in AlignmentService

**Category:** Dead Code
**Severity:** Medium
**Effort:** Small (30 minutes)

**Description:**
`AlignmentService` contains `TryExtractChapterNumber` and `EnumerateLabelCandidates` which duplicate functionality in `ChapterContext`.

**Impact:**
- Code duplication
- Unclear which to use

**Recommendation:**
Remove duplicates from `AlignmentService`, delegate to shared utility.

**Dependencies:** AUD-004 (same root cause)

---

### AUD-013: ASR Buffer Preparation Split

**Category:** Scattered Logic
**Severity:** Medium
**Effort:** Medium (4 hours)

**Description:**
ASR buffer preparation (mono downmix) is implemented in two different locations with potentially divergent behavior.

**Impact:**
- Risk of inconsistent audio preprocessing
- Maintenance of two implementations

**Recommendation:**
Consolidate into single `AsrAudioPreparer` utility class.

**Dependencies:** None

---

### AUD-014: MFA Artifacts Location

**Category:** Scattered Logic
**Severity:** Medium
**Effort:** Small (1 hour)

**Description:**
MFA-related artifacts (`MfaCorpusPreparer`, `MfaDictionaryBuilder`, etc.) are located outside the `Application/Mfa/` folder, reducing cohesion.

**Impact:**
- Reduced discoverability
- Inconsistent organization

**Recommendation:**
Move MFA-related files to `Application/Mfa/` to match other subsystem organization.

**Dependencies:** None

---

### AUD-015 through AUD-018: Questionable Interfaces

**Category:** Over-Abstraction
**Severity:** Medium
**Effort:** Small (1-2 hours each)

**Description:**
Four interfaces have single implementations and questionable value:
- `IMfaService` (AUD-015)
- `IBookParser` (AUD-016)
- `IBookIndexer` (AUD-017)
- `IBookCache` (AUD-018)

**Impact:**
- Unnecessary indirection
- More code to maintain

**Recommendation:**
Evaluate each for DI/testing value. If not needed for mocking in tests, consider removing interface and using concrete class directly.

**Dependencies:** None

---

### AUD-019: Ams.UI.Avalonia Dormant

**Category:** Project Status
**Severity:** Medium
**Effort:** Small (10 minutes)

**Description:**
Ams.UI.Avalonia is an empty skeleton with no functionality. Only 3 source files with empty implementations.

**Impact:**
- Solution clutter
- May confuse developers about UI strategy

**Recommendation:**
Archive to `archive/` directory or remove from solution entirely.

**Dependencies:** None

---

### AUD-020: Ams.Tests Failing Tests

**Category:** Project Status
**Severity:** Medium
**Effort:** Medium (2-4 hours)

**Description:**
2 of 46 tests fail in Ams.Tests due to FFmpeg filter configuration issues. Same root cause as AUD-007.

**Impact:**
- Incomplete test coverage
- Reduced confidence in test suite

**Recommendation:**
Fix as part of AUD-007.

**Dependencies:** AUD-007

---

### AUD-021: DateTime Warning in Validation.razor

**Category:** Technical Debt
**Severity:** Medium
**Effort:** Small (15 minutes)

**Description:**
`Validation.razor(365,83)` has analyzer warning: "Replace DateTime usage with DateTimeOffset"

**Impact:**
- Build warning noise
- Potential timezone issues

**Recommendation:**
Replace `DateTime` with `DateTimeOffset` as suggested.

**Dependencies:** None

---

### AUD-022: Inconsistent Document Loading

**Category:** Technical Debt
**Severity:** Medium
**Effort:** Large (24+ hours)

**Description:**
Document loading patterns vary across subsystems:
- Some use lazy loading via DocumentSlot
- Some load eagerly
- Caching strategies differ

**Impact:**
- Inconsistent behavior
- Hard to reason about performance
- Potential memory issues

**Recommendation:**
Standardize on DocumentSlot pattern across all subsystems. Define clear loading policies.

**Dependencies:** None (architectural decision)

---

### AUD-023 through AUD-025: Low-Priority Dead Code

**Category:** Dead Code
**Severity:** Low
**Effort:** Small

**Description:**
- AUD-023: InspectDocX analysis tool - one-time utility, 17 lines
- AUD-024: Ams.UI.Avalonia skeleton - 45 lines
- AUD-025: Commented-out SnapToEnergyAuto reference

**Recommendation:**
Archive or remove during general cleanup.

**Dependencies:** None

---

### AUD-026: Scattered Validation Files

**Category:** Scattered Logic
**Severity:** Low
**Effort:** Medium (4 hours)

**Description:**
Validation-related files are spread across multiple locations in the codebase.

**Impact:**
- Reduced discoverability
- Unclear responsibility boundaries

**Recommendation:**
Consolidate into `Application/Validation/` folder structure.

**Dependencies:** None

---

### AUD-027 through AUD-029: Low-Priority Project Issues

**Category:** Project Status
**Severity:** Low
**Effort:** Varies

**Description:**
- AUD-027: InspectDocX should be archived
- AUD-028: Web stack needs auth for production (Large effort)
- AUD-029: Missing MFA integration tests (Large effort)

**Recommendation:**
Address as part of future milestones.

**Dependencies:** None

---

### AUD-030: Runtime Subsystem Oversized

**Category:** Technical Debt
**Severity:** Low
**Effort:** Large (40+ hours)

**Description:**
Runtime subsystem contains 28 files, making it the largest subsystem in Ams.Core. Could benefit from decomposition.

**Impact:**
- Large subsystem harder to navigate
- Multiple concerns bundled together

**Recommendation:**
Consider splitting into Runtime/Core, Runtime/Workspace, Runtime/Artifacts in future milestone.

**Dependencies:** AUD-003 (AlignmentService split would inform approach)

---

### AUD-031: Prosody Patterns Need Standardization

**Category:** Technical Debt
**Severity:** Low
**Effort:** Medium (8 hours)

**Description:**
Prosody subsystem (9 files) has inconsistent patterns for pause handling and dynamics.

**Impact:**
- Inconsistent API
- Harder to extend

**Recommendation:**
Standardize patterns in future prosody enhancement milestone.

**Dependencies:** None

---

## Quick Wins Summary

Issues that can be resolved in under 1 hour with minimal risk:

| ID | Description | Time |
|----|-------------|------|
| AUD-006 | Delete OverlayTest project | 5 min |
| AUD-010 | Delete Class1.cs from Ams.Web.Shared | 5 min |
| AUD-008 | Delete 4 Wn*.cs placeholders | 15 min |
| AUD-009 | Delete ManifestV2.cs | 15 min |
| AUD-005 | Delete IAudioService + AudioService | 15 min |
| AUD-021 | Fix DateTime warning | 15 min |
| **Total Quick Wins** | | ~70 min |

**Lines Removed by Quick Wins:** ~90 lines

---

## Effort Estimates Summary

| Effort | Issues | Description |
|--------|--------|-------------|
| Small (< 4 hours) | 18 | Quick fixes, file deletions |
| Medium (4-16 hours) | 9 | Refactoring, test fixes |
| Large (16+ hours) | 4 | Architectural changes |

---

*Generated: 2025-12-28*
