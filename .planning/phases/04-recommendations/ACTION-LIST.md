# Action List

**Master prioritized action list for AMS codebase improvements**

Generated: 2025-12-30
Source: All Phase 3 and Phase 4 deliverables

---

## 1. Executive Summary

| Metric | Value |
|--------|-------|
| Total Actions | 45 |
| Total Effort | ~55 hours |
| Issues Addressed | 31/31 |
| Health Improvement | 6.8 -> 8.0 (+1.2) |

### Effort Breakdown

| Timeframe | Actions | Effort | Health Impact |
|-----------|---------|--------|---------------|
| Immediate (This Week) | 10 | ~2 hours | +0.3 |
| Short-Term (Next 2 Weeks) | 12 | ~12 hours | +0.4 |
| Medium-Term (Next Month) | 15 | ~35 hours | +0.5 |
| Long-Term / Deferred | 8 | ~40+ hours | Future |

---

## 2. Immediate Actions (Do This Week)

**Total Time: ~2 hours**
**Health Impact: +0.3 (6.8 -> 7.1)**

These are safe, quick wins requiring no discussion or dependencies.

### 2.1 Delete Broken Project (5 minutes)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-006 |
| **Action** | Delete OverlayTest project |
| **File(s)** | `analysis/OverlayTest/` (entire directory) |
| **Command** | `Remove-Item -Recurse -Force analysis/OverlayTest` |
| **Risk** | NONE (build broken, no value) |

- [ ] Delete OverlayTest directory
- [ ] Remove from solution file if present

---

### 2.2 Delete Empty Placeholders (15 minutes)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-008 |
| **Action** | Delete 4 empty Whisper.NET placeholder files |
| **File(s)** | `host/Ams.Core/Services/Integrations/ASR/WhisperNet/*.cs` |
| **Command** | `git rm host/Ams.Core/Services/Integrations/ASR/WhisperNet/Wn*.cs` |
| **Risk** | NONE (empty classes, zero callers) |

- [ ] Delete WnModel.cs (8 lines)
- [ ] Delete WnSession.cs (8 lines)
- [ ] Delete WnTranscriber.cs (8 lines)
- [ ] Delete WnUtils.cs (8 lines)
- [ ] Build and verify: `dotnet build host/Ams.Core`

---

### 2.3 Delete Template Artifact (5 minutes)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-010 |
| **Action** | Delete empty Class1.cs |
| **File(s)** | `host/Ams.Web.Shared/Class1.cs` |
| **Command** | `git rm host/Ams.Web.Shared/Class1.cs` |
| **Risk** | NONE (template leftover, 5 lines) |

- [ ] Delete Class1.cs
- [ ] Build and verify: `dotnet build host/Ams.Web.Shared`

---

### 2.4 Delete Superseded Manifest (15 minutes)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-009 |
| **Action** | Delete ManifestV2.cs |
| **File(s)** | `host/Ams.Core/Pipeline/ManifestV2.cs` |
| **Command** | `git rm host/Ams.Core/Pipeline/ManifestV2.cs` |
| **Risk** | NONE (superseded format, zero callers) |

- [ ] Verify no references: `grep -r "ManifestV2" --include="*.cs" host/`
- [ ] Delete ManifestV2.cs
- [ ] Build and verify

---

### 2.5 Delete Empty Service Interface (15 minutes)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-005 |
| **Action** | Delete IAudioService and AudioService |
| **File(s)** | `host/Ams.Core/Services/Interfaces/IAudioService.cs`, `host/Ams.Core/Services/AudioService.cs` |
| **Command** | `git rm host/Ams.Core/Services/Interfaces/IAudioService.cs host/Ams.Core/Services/AudioService.cs` |
| **Risk** | NONE (empty implementation, never registered) |

- [ ] Verify no consumers: `grep -r "IAudioService\|AudioService" --include="*.cs" host/`
- [ ] Delete IAudioService.cs (5 lines)
- [ ] Delete AudioService.cs (13 lines)
- [ ] Build and verify

---

### 2.6 Fix DateTime Warning (15 minutes)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-021 |
| **Action** | Replace DateTime with DateTimeOffset |
| **File(s)** | `host/Ams.Web.Client/Pages/Validation.razor` (line 365) |
| **Risk** | LOW (simple type change) |

- [ ] Edit Validation.razor line 365
- [ ] Replace DateTime usage with DateTimeOffset
- [ ] Build and verify warning resolved

---

### 2.7 Remove IMfaService Interface (30 minutes)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-015 |
| **Action** | Remove unnecessary interface |
| **File(s)** | `host/Ams.Core/Application/Mfa/MfaService.cs` |
| **Risk** | LOW (interface not used in DI or tests) |

- [ ] Verify no DI registration for IMfaService
- [ ] Verify no test mocking of IMfaService
- [ ] Remove IMfaService interface definition from MfaService.cs
- [ ] Update any callers to use MfaService directly
- [ ] Build and verify

---

### 2.8 Delete Demo Runner (15 minutes)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-001 |
| **Action** | Delete DspDemoRunner.cs |
| **File(s)** | `host/Ams.Core/Audio/DspDemoRunner.cs` |
| **Command** | `git rm host/Ams.Core/Audio/DspDemoRunner.cs` |
| **Risk** | NONE (demo code, zero callers) |

- [ ] Verify no callers: `grep -r "DspDemoRunner" --include="*.cs" host/`
- [ ] Delete DspDemoRunner.cs (141 lines)
- [ ] Build and verify

---

### 2.9 Delete Unused Timeline Builder (15 minutes)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-002 |
| **Action** | Delete SentenceTimelineBuilder.cs |
| **File(s)** | `host/Ams.Core/Audio/SentenceTimelineBuilder.cs` |
| **Command** | `git rm host/Ams.Core/Audio/SentenceTimelineBuilder.cs` |
| **Risk** | NONE (zero callers, superseded) |

- [ ] Verify no callers: `grep -r "SentenceTimelineBuilder" --include="*.cs" host/`
- [ ] Delete SentenceTimelineBuilder.cs (182 lines)
- [ ] Build and verify

---

### 2.10 Archive Dormant Projects (15 minutes)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-019, AUD-023, AUD-024, AUD-027 |
| **Action** | Archive Ams.UI.Avalonia and InspectDocX |
| **File(s)** | `host/Ams.UI.Avalonia/`, `out/InspectDocX/` |
| **Risk** | NONE (no active use, preserves code) |

- [ ] Create archive directory: `mkdir archive`
- [ ] Move Ams.UI.Avalonia: `git mv host/Ams.UI.Avalonia archive/`
- [ ] Move InspectDocX: `git mv out/InspectDocX archive/`
- [ ] Update solution file to remove projects
- [ ] Build and verify

---

### Immediate Actions Summary

| # | Action | Issue ID | Lines | Time |
|---|--------|----------|-------|------|
| 1 | Delete OverlayTest | AUD-006 | 24 | 5 min |
| 2 | Delete Wn*.cs placeholders | AUD-008 | 32 | 15 min |
| 3 | Delete Class1.cs | AUD-010 | 5 | 5 min |
| 4 | Delete ManifestV2.cs | AUD-009 | 19 | 15 min |
| 5 | Delete IAudioService + AudioService | AUD-005 | 18 | 15 min |
| 6 | Fix DateTime warning | AUD-021 | - | 15 min |
| 7 | Remove IMfaService | AUD-015 | ~10 | 30 min |
| 8 | Delete DspDemoRunner | AUD-001 | 141 | 15 min |
| 9 | Delete SentenceTimelineBuilder | AUD-002 | 182 | 15 min |
| 10 | Archive dormant projects | AUD-019,23,24,27 | 62 | 15 min |
| | **Total** | | **~493** | **~2h** |

---

## 3. Short-Term Actions (Next 2 Weeks)

**Total Time: ~12 hours**
**Health Impact: +0.4 (7.1 -> 7.5)**

### 3.1 Extract ChapterLabelResolver Utility (3 hours)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-004, AUD-012 |
| **Action** | Extract section resolution to shared utility |
| **Effort** | 3 hours |
| **Dependencies** | None |

**Current State:**
- `TryExtractChapterNumber` duplicated in ChapterContext and AlignmentService
- `EnumerateLabelCandidates` duplicated in same files
- ~40 lines of duplicate code

**Target:**
- Create `Common/ChapterLabelResolver.cs`
- Remove duplicates from ChapterContext
- Remove duplicates from AlignmentService

- [ ] Create `host/Ams.Core/Common/ChapterLabelResolver.cs`
- [ ] Move `TryExtractChapterNumber` with compiled regex
- [ ] Move `EnumerateLabelCandidates`
- [ ] Add `ResolveSection` wrapper method
- [ ] Update ChapterContext to use ChapterLabelResolver
- [ ] Update AlignmentService to use ChapterLabelResolver
- [ ] Add unit tests for ChapterLabelResolver
- [ ] Build and run all tests

---

### 3.2 Relocate MFA Artifacts (1 hour)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-014 |
| **Action** | Move MFA models to Application/Mfa/Models |
| **Effort** | 1 hour |
| **Dependencies** | None |

**Files to Move:**
- `Artifacts/Alignment/MfaChapterContext.cs` -> `Application/Mfa/Models/`
- `Artifacts/Alignment/MfaCommandResult.cs` -> `Application/Mfa/Models/`

- [ ] Create `host/Ams.Core/Application/Mfa/Models/` directory
- [ ] Move MfaChapterContext.cs
- [ ] Move MfaCommandResult.cs
- [ ] Update namespace: `Ams.Core.Artifacts.Alignment` -> `Ams.Core.Application.Mfa.Models`
- [ ] Update using statements in all callers
- [ ] Build and verify

---

### 3.3 Fix FFmpeg Filter Tests (3 hours)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-007, AUD-020 |
| **Action** | Fix failing AudioProcessorFilterTests |
| **Effort** | 3 hours |
| **Dependencies** | None |

**Failing Tests:**
- `Trim_ReturnsExpectedSegment` - FFmpeg filter syntax error
- `FadeIn_SetsLeadingSamplesToZero` - FFmpeg filter syntax error

**Root Cause:** Likely locale-dependent decimal formatting in filter strings

- [ ] Investigate FFmpeg filter string construction
- [ ] Add `CultureInfo.InvariantCulture` for decimal formatting
- [ ] Fix Trim filter test
- [ ] Fix FadeIn filter test
- [ ] Run all tests to verify 46/46 pass

---

### 3.4 Consolidate ASR Buffer Preparation (4 hours)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-013 |
| **Action** | Create AsrAudioPreparer utility |
| **Effort** | 4 hours |
| **Dependencies** | None |

**Current State:**
- AsrService uses FFmpeg filter graph (high quality)
- AsrProcessor uses simple averaging (lower quality)

**Target:**
- Create `Audio/AsrAudioPreparer.cs`
- Single implementation using FFmpeg
- Simple fallback for non-FFmpeg environments

- [ ] Create `host/Ams.Core/Audio/AsrAudioPreparer.cs`
- [ ] Move `PrepareForAsr` from AsrService
- [ ] Move `BuildMonoPanClause` from AsrService
- [ ] Add `DownmixToMonoSimple` fallback
- [ ] Update AsrService to use AsrAudioPreparer
- [ ] Update AsrProcessor to use AsrAudioPreparer
- [ ] Add unit tests
- [ ] Build and run all tests

---

### 3.5 Remove Unused AudioProcessor Methods (1 hour)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-011, AUD-025 |
| **Action** | Remove unused methods from AudioProcessor |
| **Effort** | 1 hour |
| **Dependencies** | Immediate Actions complete |

**Methods to Remove:**
- `AdjustVolume` (~15 lines)
- `FadeOut` (~15 lines)
- `EncodeWav` (~10 lines)
- `NormalizeLoudness` (~40 lines)
- `AnalyzeGap` (~30 lines)
- `FindSpeechEndFromGap` (~25 lines)
- `SnapToEnergyAuto` (~25 lines)
- Remove commented-out SnapToEnergyAuto reference

- [ ] Verify each method has zero callers (grep)
- [ ] Remove AdjustVolume from AudioProcessor.cs
- [ ] Remove FadeOut from AudioProcessor.cs
- [ ] Remove EncodeWav from AudioProcessor.cs
- [ ] Remove NormalizeLoudness from AudioProcessor.cs
- [ ] Remove AnalyzeGap from AudioProcessor.Analysis.cs
- [ ] Remove FindSpeechEndFromGap from AudioProcessor.Analysis.cs
- [ ] Remove SnapToEnergyAuto from AudioProcessor.Analysis.cs
- [ ] Build and verify

---

### Short-Term Actions Summary

| # | Action | Issue ID | Effort |
|---|--------|----------|--------|
| 1 | Extract ChapterLabelResolver | AUD-004, AUD-012 | 3h |
| 2 | Relocate MFA artifacts | AUD-014 | 1h |
| 3 | Fix FFmpeg filter tests | AUD-007, AUD-020 | 3h |
| 4 | Consolidate ASR buffer prep | AUD-013 | 4h |
| 5 | Remove unused AudioProcessor methods | AUD-011, AUD-025 | 1h |
| | **Total** | | **12h** |

---

## 4. Medium-Term Actions (Next Month)

**Total Time: ~35 hours**
**Health Impact: +0.5 (7.5 -> 8.0)**

### 4.1 AlignmentService Decomposition (20 hours)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-003 |
| **Action** | Split AlignmentService into focused services |
| **Effort** | 16-24 hours |
| **Dependencies** | ChapterLabelResolver extraction complete |

**Phase 1: Create AnchorComputeService (4-6 hours)**
- [ ] Create `Services/Alignment/IAnchorComputeService.cs`
- [ ] Create `Services/Alignment/AnchorComputeService.cs`
- [ ] Move `ComputeAnchorsAsync` from AlignmentService
- [ ] Move `BuildPolicy` from AlignmentService
- [ ] Move `BuildAnchorDocument` from AlignmentService
- [ ] Add unit tests
- [ ] Register in DI

**Phase 2: Create TranscriptIndexService (6-8 hours)**
- [ ] Create `Services/Alignment/ITranscriptIndexService.cs`
- [ ] Create `Services/Alignment/TranscriptIndexService.cs`
- [ ] Move `BuildTranscriptIndexAsync` from AlignmentService
- [ ] Move `BuildWordOperations` from AlignmentService
- [ ] Move `BuildRollups` from AlignmentService
- [ ] Move phoneme view builders from AlignmentService
- [ ] Move `BuildFallbackWindows` from AlignmentService
- [ ] Add unit tests
- [ ] Register in DI

**Phase 3: Create TranscriptHydrationService (3-4 hours)**
- [ ] Create `Services/Alignment/ITranscriptHydrationService.cs`
- [ ] Create `Services/Alignment/TranscriptHydrationService.cs`
- [ ] Move `HydrateTranscriptAsync` from AlignmentService
- [ ] Move `BuildHydratedTranscript` from AlignmentService
- [ ] Move `BuildParagraphScript` from AlignmentService
- [ ] Move `ComputeTiming` from AlignmentService
- [ ] Add unit tests
- [ ] Register in DI

**Phase 4: Create AlignmentService Facade (1-2 hours)**
- [ ] Refactor AlignmentService to inject 3 new services
- [ ] Delegate all public methods to appropriate service
- [ ] Verify IAlignmentService contract unchanged
- [ ] Run all existing tests

**Phase 5: Cleanup (1-2 hours)**
- [ ] Remove shared utility methods (now in ChapterLabelResolver)
- [ ] Remove path resolution helpers (if appropriate)
- [ ] Run full test suite
- [ ] Verify pipeline still works end-to-end

---

### 4.2 Review IBook* Interfaces (2 hours)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-016, AUD-017, AUD-018 |
| **Action** | Verify interfaces provide value, keep or simplify |
| **Effort** | 2 hours |
| **Dependencies** | None |

**Decision:** After analysis, these interfaces should be KEPT:
- `IBookParser` - DI registered, enables future swapping
- `IBookIndexer` - DI registered, enables future swapping
- `IBookCache` - DI registered, enables future swapping

- [ ] Document decision in codebase (comments or ADR)
- [ ] No code changes needed

---

### 4.3 Consolidate Validation Files (4 hours)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-026 |
| **Action** | Consolidate validation files to single location |
| **Effort** | 4 hours |
| **Dependencies** | None |

**Current Locations:**
- `Services/ValidationService.cs`
- `Processors/Validation/ValidationReportBuilder.cs`
- `Validation/ScriptValidator.cs`
- `Validation/ValidationModels.cs`
- `Artifacts/Validation/ValidationReportModels.cs`

**Target:**
- `Application/Validation/ValidationService.cs`
- `Application/Validation/ValidationReportBuilder.cs`
- `Application/Validation/ScriptValidator.cs`
- `Application/Validation/Models/ValidationModels.cs`
- `Application/Validation/Models/ValidationReportModels.cs`

- [ ] Create `Application/Validation/` directory
- [ ] Create `Application/Validation/Models/` subdirectory
- [ ] Move each file with namespace update
- [ ] Update all using statements
- [ ] Build and verify

---

### 4.4 Standardize Prosody Patterns (6 hours)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-031 |
| **Action** | Standardize patterns in Prosody subsystem |
| **Effort** | 6 hours |
| **Dependencies** | None |

**Areas to Address:**
- Consistent naming conventions
- Standardized model patterns
- Unified service interfaces
- Consistent option handling

- [ ] Review all 9 Prosody files
- [ ] Document current patterns
- [ ] Identify inconsistencies
- [ ] Create standardization plan
- [ ] Apply changes incrementally
- [ ] Add/update tests as needed

---

### 4.5 Improve Test Coverage (3 hours)

| Item | Details |
|------|---------|
| **Issue ID** | Inferred from audit |
| **Action** | Add tests for new utilities and services |
| **Effort** | 3 hours |
| **Dependencies** | Short-term actions complete |

**Priority Test Areas:**
- ChapterLabelResolver utility
- AsrAudioPreparer utility
- New alignment services (after decomposition)

- [ ] Add ChapterLabelResolver unit tests
- [ ] Add AsrAudioPreparer unit tests
- [ ] Add AnchorComputeService unit tests
- [ ] Add TranscriptIndexService unit tests
- [ ] Add TranscriptHydrationService unit tests

---

### Medium-Term Actions Summary

| # | Action | Issue ID | Effort |
|---|--------|----------|--------|
| 1 | AlignmentService decomposition | AUD-003 | 20h |
| 2 | Review IBook* interfaces | AUD-016-18 | 2h |
| 3 | Consolidate validation files | AUD-026 | 4h |
| 4 | Standardize Prosody patterns | AUD-031 | 6h |
| 5 | Improve test coverage | - | 3h |
| | **Total** | | **35h** |

---

## 5. Long-Term / Deferred Actions

**These actions are explicitly deferred** - to be revisited in future milestones.

### 5.1 Runtime Subsystem Decomposition (DEFERRED)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-030 |
| **Action** | Split Runtime subsystem (28 files) |
| **Effort** | 40+ hours |
| **Status** | DEFERRED |
| **Reason** | No immediate pain point, high effort, low ROI |

**When to Reconsider:**
- Adding significant new Runtime features
- Performance issues with context management
- New host types beyond CLI/Web

---

### 5.2 Web Stack Authentication (DEFERRED)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-028 |
| **Action** | Add authentication for multi-user deployment |
| **Effort** | 40+ hours |
| **Status** | DEFERRED |
| **Reason** | Current use is single-user, no multi-user requirements |

**When to Reconsider:**
- Multi-user deployment planned
- External access requirements
- Security audit requirements

---

### 5.3 MFA Integration Tests (DEFERRED)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-029 |
| **Action** | Add integration tests for MFA workflow |
| **Effort** | 20+ hours |
| **Status** | DEFERRED |
| **Reason** | Requires MFA environment setup, complex external dependency |

**When to Reconsider:**
- MFA workflow changes planned
- CI/CD pipeline improvements
- Regression issues with MFA

---

### 5.4 Document Loading Pattern Unification (DEFERRED)

| Item | Details |
|------|---------|
| **Issue ID** | AUD-022 |
| **Action** | Standardize document loading across subsystems |
| **Effort** | 24+ hours |
| **Status** | DEFERRED |
| **Reason** | No immediate issues, large scope, needs architectural decision |

**When to Reconsider:**
- Performance issues with document loading
- Memory pressure in large books
- New caching requirements

---

### Deferred Actions Summary

| # | Action | Issue ID | Effort | Reason |
|---|--------|----------|--------|--------|
| 1 | Runtime decomposition | AUD-030 | 40h+ | Low ROI |
| 2 | Web authentication | AUD-028 | 40h+ | Not needed yet |
| 3 | MFA integration tests | AUD-029 | 20h+ | Complex setup |
| 4 | Document loading unification | AUD-022 | 24h+ | Needs arch decision |

---

## 6. Dependency Graph

### 6.1 Action Dependencies

```
IMMEDIATE ACTIONS (Week 1)
├── Delete OverlayTest ─────────────────┐
├── Delete Wn*.cs placeholders ─────────┼──► No dependencies
├── Delete Class1.cs ───────────────────┤
├── Delete ManifestV2.cs ───────────────┤
├── Delete IAudioService + AudioService ┤
├── Fix DateTime warning ───────────────┤
├── Remove IMfaService ─────────────────┤
├── Delete DspDemoRunner ───────────────┤
├── Delete SentenceTimelineBuilder ─────┤
└── Archive dormant projects ───────────┘
                    │
                    ▼
SHORT-TERM ACTIONS (Weeks 2-3)
├── Extract ChapterLabelResolver ────────────────────┐
│                                                    │
├── Relocate MFA artifacts ─────────────────────────►├── Independent
│                                                    │
├── Fix FFmpeg filter tests ────────────────────────►│
│                                                    │
├── Consolidate ASR buffer prep ────────────────────►│
│                                                    │
└── Remove unused AudioProcessor methods ───────────►└── After immediate complete
                    │
                    ▼
MEDIUM-TERM ACTIONS (Weeks 4-8)
├── AlignmentService decomposition ◄─── Requires ChapterLabelResolver
│   ├── Phase 1: AnchorComputeService
│   ├── Phase 2: TranscriptIndexService (depends on Phase 1)
│   ├── Phase 3: TranscriptHydrationService
│   ├── Phase 4: AlignmentService facade (depends on Phases 1-3)
│   └── Phase 5: Cleanup
│
├── Review IBook* interfaces ───────────────────────► Independent
├── Consolidate validation files ───────────────────► Independent
├── Standardize Prosody patterns ───────────────────► Independent
└── Improve test coverage ◄──────────────────────────┘
         Depends on new utilities and services
```

### 6.2 Critical Path

```
Week 1                Week 2              Week 3              Week 4-8
┌──────────┐         ┌──────────┐        ┌──────────┐        ┌──────────────┐
│Immediate │  ────►  │ChapterLbl│  ────► │ Continue │  ────► │Alignment     │
│Actions   │         │Resolver  │        │ Short    │        │Service Split │
│(2 hours) │         │(3 hours) │        │ Term     │        │(20 hours)    │
└──────────┘         └──────────┘        └──────────┘        └──────────────┘
                          ▲
                          │
                    Critical path item
                    (blocks AlignmentService work)
```

### 6.3 Parallel Execution Opportunities

These action groups can be executed in parallel:

**Parallel Group A (After Immediate):**
- Relocate MFA artifacts
- Fix FFmpeg filter tests
- Consolidate ASR buffer prep

**Parallel Group B (Medium-Term):**
- Review IBook* interfaces
- Consolidate validation files
- Standardize Prosody patterns

---

## 7. Risk Assessment

### 7.1 Risk Matrix by Action Category

| Category | Risk Level | Mitigation |
|----------|------------|------------|
| **Immediate Actions** | NONE | All verified as orphaned code |
| **Short-Term Consolidations** | LOW | Extract then refactor, maintain tests |
| **AlignmentService Split** | MEDIUM | Phase approach, facade for compatibility |
| **Deferred Actions** | N/A | Not executing now |

### 7.2 Rollback Strategies

**For Dead Code Removal:**
- Git commits allow easy revert
- No functional impact (verified orphans)

**For Utility Extraction:**
- Keep original code until new utility verified
- Run full test suite after each change

**For AlignmentService Split:**
- Facade maintains existing API
- Split in phases with tests between each
- Full pipeline test after each phase

### 7.3 Specific Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| False positive dead code | LOW | LOW | Grep verification before deletion |
| AlignmentService regression | MEDIUM | HIGH | Comprehensive tests, phased approach |
| Namespace confusion after moves | LOW | LOW | IDE refactoring tools, search/replace |
| FFmpeg test fix incomplete | MEDIUM | LOW | Culture-invariant formatting |

---

## 8. Success Metrics

### 8.1 Health Score Checkpoints

| Checkpoint | Target Score | Validation |
|------------|--------------|------------|
| After Immediate Actions | 7.1 | Dead code metrics improved |
| After Short-Term Actions | 7.5 | Duplication eliminated, tests green |
| After Medium-Term Actions | 8.0 | AlignmentService split, validation consolidated |

### 8.2 Measurable Outcomes

| Metric | Current | After Immediate | After Full |
|--------|---------|-----------------|------------|
| Dead code lines | ~650 | ~150 | 0 |
| Failing tests | 2 | 2 | 0 |
| God classes | 1 | 1 | 0 |
| Duplicate code blocks | 3 | 3 | 0 |
| Build warnings | 1 | 0 | 0 |

### 8.3 Validation Checkpoints

**After Each Phase:**
- [ ] `dotnet build` succeeds with 0 warnings
- [ ] `dotnet test` passes all tests
- [ ] Pipeline runs end-to-end successfully

**After Immediate Actions:**
- [ ] Solution file clean (no broken project references)
- [ ] No empty placeholder files remain
- [ ] Dead code count reduced by ~500 lines

**After Short-Term Actions:**
- [ ] All 46 tests pass (was 44/46)
- [ ] No duplicate section resolution code
- [ ] Single ASR buffer preparation path

**After Medium-Term Actions:**
- [ ] AlignmentService < 100 lines (was 681)
- [ ] 4 focused alignment services
- [ ] Validation files in single location

---

## 9. Issue Disposition Summary

**All 31 issues from ISSUES-CATALOGUE.md are addressed:**

### Addressed in Immediate Actions (10 issues)

| Issue ID | Description | Disposition |
|----------|-------------|-------------|
| AUD-001 | DspDemoRunner.cs | DELETE |
| AUD-002 | SentenceTimelineBuilder.cs | DELETE |
| AUD-005 | IAudioService empty | DELETE |
| AUD-006 | OverlayTest broken | DELETE |
| AUD-008 | Wn*.cs placeholders | DELETE |
| AUD-009 | ManifestV2.cs | DELETE |
| AUD-010 | Class1.cs | DELETE |
| AUD-015 | IMfaService | REMOVE |
| AUD-019 | UI.Avalonia dormant | ARCHIVE |
| AUD-021 | DateTime warning | FIX |

### Addressed in Short-Term Actions (6 issues)

| Issue ID | Description | Disposition |
|----------|-------------|-------------|
| AUD-004 | Section resolution duplication | EXTRACT |
| AUD-007 | FFmpeg filter tests | FIX |
| AUD-011 | Unused AudioProcessor methods | DELETE |
| AUD-012 | Duplicate methods in AlignmentService | EXTRACT |
| AUD-013 | ASR buffer prep split | CONSOLIDATE |
| AUD-014 | MFA artifacts location | RELOCATE |
| AUD-020 | Ams.Tests failing | FIX (with AUD-007) |
| AUD-025 | Commented SnapToEnergyAuto | DELETE |

### Addressed in Medium-Term Actions (8 issues)

| Issue ID | Description | Disposition |
|----------|-------------|-------------|
| AUD-003 | AlignmentService god class | DECOMPOSE |
| AUD-016 | IBookParser | KEEP (documented) |
| AUD-017 | IBookIndexer | KEEP (documented) |
| AUD-018 | IBookCache | KEEP (documented) |
| AUD-026 | Validation files scattered | CONSOLIDATE |
| AUD-031 | Prosody patterns | STANDARDIZE |

### Addressed as Deferred (4 issues)

| Issue ID | Description | Disposition |
|----------|-------------|-------------|
| AUD-022 | Document loading inconsistent | DEFER |
| AUD-028 | Web auth needed | DEFER |
| AUD-029 | Missing MFA integration tests | DEFER |
| AUD-030 | Runtime oversized | DEFER |

### Addressed via Archive (3 issues)

| Issue ID | Description | Disposition |
|----------|-------------|-------------|
| AUD-023 | InspectDocX | ARCHIVE |
| AUD-024 | UI.Avalonia skeleton | ARCHIVE |
| AUD-027 | InspectDocX archival | ARCHIVE |

---

*Generated: 2025-12-30*
*Source: ISSUES-CATALOGUE.md, PRUNING-PLAN.md, CONSOLIDATION-PLAN.md, REFACTORING-CANDIDATES.md*
