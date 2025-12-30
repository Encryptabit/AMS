# Refactoring Candidates

**Detailed decomposition designs for major refactoring opportunities**

Generated: 2025-12-30
Source: Phase 3 artifacts (RESPONSIBILITY-MAP.md, ISSUES-CATALOGUE.md, CORE-SUBSYSTEMS.md)

---

## Executive Summary

| Candidate | Issue ID | Lines | Effort | Risk | Recommendation |
|-----------|----------|-------|--------|------|----------------|
| AlignmentService Decomposition | AUD-003 | 681 | 16-24 hours | MEDIUM | **DO NOW** |
| Runtime Subsystem Decomposition | AUD-030 | 28 files | 40+ hours | HIGH | **DEFER** |
| Over-Abstraction Removal | AUD-005,15-18 | ~60 | 4-8 hours | LOW | **DO NOW** |

---

## 1. AlignmentService Decomposition

**Issue ID:** AUD-003
**Priority:** HIGH
**Current Size:** 681 lines
**Effort:** 16-24 hours
**Risk:** MEDIUM (core alignment logic)

### 1.1 Current Responsibilities

AlignmentService currently handles **4 distinct responsibilities**:

| # | Responsibility | Public Method | Lines (est.) | Calls |
|---|---------------|---------------|--------------|-------|
| 1 | Anchor computation | `ComputeAnchorsAsync` | ~100 | From commands |
| 2 | Transcript indexing | `BuildTranscriptIndexAsync` | ~150 | From commands |
| 3 | Transcript hydration | `HydrateTranscriptAsync` | ~120 | From commands |
| 4 | Section resolution | (private helpers) | ~40 | Internal |

### 1.2 Method Inventory

**Current AlignmentService.cs (681 lines):**

| Method | Lines | Purpose | Target Service |
|--------|-------|---------|----------------|
| `ComputeAnchorsAsync` | 26-56 | Public: Compute anchor points | AnchorComputeService |
| `BuildTranscriptIndexAsync` | 58-148 | Public: Build transcript index | TranscriptIndexService |
| `HydrateTranscriptAsync` | 150-161 | Public: Hydrate transcript | TranscriptHydrationService |
| `RequireBookAndAsr` | 163-168 | Private: Load dependencies | Shared utility |
| `BuildPolicy` | 170-183 | Private: Create anchor policy | AnchorComputeService |
| `BuildAnchorDocument` | 185-226 | Private: Create anchor output | AnchorComputeService |
| `TryExtractChapterNumber` | 228-244 | Private: Parse chapter from label | **ChapterLabelResolver** |
| `EnumerateLabelCandidates` | 246-264 | Private: Generate label options | **ChapterLabelResolver** |
| `BuildWordOperations` | 266-342 | Private: Create word alignments | TranscriptIndexService |
| `BuildRollups` | 344-406 | Private: Sentence/paragraph rollup | TranscriptIndexService |
| `BuildBookPhonemeView` | 408-426 | Private: Book phoneme array | TranscriptIndexService |
| `BuildAsrPhonemeViewAsync` | 428-462 | Private: ASR phoneme array | TranscriptIndexService |
| `BuildFallbackWindows` | 464-506 | Private: Fallback alignment windows | TranscriptIndexService |
| `BuildHydratedTranscript` | 508-627 | Private: Create hydrated output | TranscriptHydrationService |
| `BuildParagraphScript` | 629-647 | Private: Paragraph text builder | TranscriptHydrationService |
| `ComputeTiming` | 649-665 | Private: Calculate timing range | TranscriptHydrationService |
| `ResolveDefaultAudioPath` | 667-676 | Private: Default audio path | Shared utility |
| `ResolveDefaultBookIndexPath` | 678-679 | Private: Default book index path | Shared utility |

### 1.3 Proposed Split

**New structure:**

```
Services/Alignment/
├── AlignmentService.cs          (thin facade, ~50 lines)
├── AnchorComputeService.cs      (~180 lines)
├── TranscriptIndexService.cs    (~280 lines)
├── TranscriptHydrationService.cs (~200 lines)
└── AlignmentOptions.cs          (unchanged)

Common/
└── ChapterLabelResolver.cs      (~50 lines, see CONSOLIDATION-PLAN.md)
```

#### 1.3.1 AnchorComputeService

**Responsibility:** Compute anchor points between book and ASR tokens.

**Methods to move:**
- `ComputeAnchorsAsync` → public entry point
- `BuildPolicy` → internal policy builder
- `BuildAnchorDocument` → internal document builder

**Dependencies:**
- `IPronunciationProvider` (from constructor)
- `AnchorPipeline` (static methods)
- `AnchorPreprocessor` (static methods)

**Interface:**
```csharp
public interface IAnchorComputeService
{
    Task<AnchorDocument> ComputeAnchorsAsync(
        ChapterContext context,
        AnchorComputationOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

#### 1.3.2 TranscriptIndexService

**Responsibility:** Build word-level transcript index with alignment operations.

**Methods to move:**
- `BuildTranscriptIndexAsync` → public entry point
- `BuildWordOperations` → internal alignment logic
- `BuildRollups` → internal sentence/paragraph grouping
- `BuildBookPhonemeView` → internal phoneme prep
- `BuildAsrPhonemeViewAsync` → internal phoneme prep
- `BuildFallbackWindows` → internal window calculation

**Dependencies:**
- `IPronunciationProvider` (from constructor)
- `IAnchorComputeService` (for anchor computation within index building)
- `TranscriptAligner` (static methods)
- `AnchorPreprocessor` (static methods)

**Interface:**
```csharp
public interface ITranscriptIndexService
{
    Task<TranscriptIndex> BuildTranscriptIndexAsync(
        ChapterContext context,
        TranscriptBuildOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

#### 1.3.3 TranscriptHydrationService

**Responsibility:** Hydrate transcript index with full text and metrics.

**Methods to move:**
- `HydrateTranscriptAsync` → public entry point
- `BuildHydratedTranscript` → internal hydration logic
- `BuildParagraphScript` → internal text builder
- `ComputeTiming` → internal timing calculation

**Dependencies:**
- None (uses only context and transcript)
- `TextDiffAnalyzer` (static methods)
- `TextNormalizer` (static methods)

**Interface:**
```csharp
public interface ITranscriptHydrationService
{
    Task<HydratedTranscript> HydrateTranscriptAsync(
        ChapterContext context,
        HydrationOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

#### 1.3.4 AlignmentService (Facade)

**Responsibility:** Thin facade maintaining backwards compatibility.

**Implementation:**
```csharp
public sealed class AlignmentService : IAlignmentService
{
    private readonly IAnchorComputeService _anchorService;
    private readonly ITranscriptIndexService _indexService;
    private readonly ITranscriptHydrationService _hydrationService;

    public AlignmentService(
        IAnchorComputeService anchorService,
        ITranscriptIndexService indexService,
        ITranscriptHydrationService hydrationService)
    {
        _anchorService = anchorService;
        _indexService = indexService;
        _hydrationService = hydrationService;
    }

    public Task<AnchorDocument> ComputeAnchorsAsync(...) =>
        _anchorService.ComputeAnchorsAsync(...);

    public Task<TranscriptIndex> BuildTranscriptIndexAsync(...) =>
        _indexService.BuildTranscriptIndexAsync(...);

    public Task<HydratedTranscript> HydrateTranscriptAsync(...) =>
        _hydrationService.HydrateTranscriptAsync(...);
}
```

### 1.4 Interface Changes

**No breaking changes to IAlignmentService** - The facade maintains the existing interface.

**New interfaces added:**
- `IAnchorComputeService`
- `ITranscriptIndexService`
- `ITranscriptHydrationService`

**DI registration changes:**
```csharp
// Old
services.AddTransient<IAlignmentService, AlignmentService>();

// New
services.AddTransient<IAnchorComputeService, AnchorComputeService>();
services.AddTransient<ITranscriptIndexService, TranscriptIndexService>();
services.AddTransient<ITranscriptHydrationService, TranscriptHydrationService>();
services.AddTransient<IAlignmentService, AlignmentService>();  // facade
```

### 1.5 Test Impact Assessment

| Test Area | Impact | Migration |
|-----------|--------|-----------|
| AlignmentService unit tests | HIGH | Split into 3 test files |
| Integration tests | LOW | No changes (uses IAlignmentService) |
| Command tests | LOW | No changes (uses IAlignmentService) |

**Testing improvements after split:**
- Each service testable in isolation
- Clearer mocking boundaries
- Easier to test edge cases per concern

### 1.6 Migration Steps

1. **Create ChapterLabelResolver** (prerequisite from CONSOLIDATION-PLAN.md)
   - Extract `TryExtractChapterNumber` and `EnumerateLabelCandidates`
   - Update ChapterContext to use it
   - Duration: 2-4 hours

2. **Create AnchorComputeService**
   - Move anchor-related methods
   - Create interface
   - Add unit tests
   - Duration: 4-6 hours

3. **Create TranscriptIndexService**
   - Move transcript building methods
   - Inject IAnchorComputeService
   - Add unit tests
   - Duration: 6-8 hours

4. **Create TranscriptHydrationService**
   - Move hydration methods
   - Add unit tests
   - Duration: 3-4 hours

5. **Create AlignmentService facade**
   - Inject all three services
   - Delegate all calls
   - Verify existing tests pass
   - Duration: 1-2 hours

6. **Update DI registration**
   - Register new services
   - Verify application works
   - Duration: 1 hour

**Total effort:** 16-24 hours

### 1.7 Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Subtle behavior change | MEDIUM | HIGH | Comprehensive test coverage before split |
| Circular dependencies | LOW | MEDIUM | Clear dependency direction (Anchor → Index → Hydration) |
| Performance regression | LOW | LOW | No algorithm changes, only code organization |
| Missing shared state | MEDIUM | MEDIUM | Extract shared utilities first |

**Overall Risk:** MEDIUM - Core alignment logic, but well-tested and understood.

---

## 2. Runtime Subsystem Decomposition

**Issue ID:** AUD-030
**Priority:** LOW
**Current Size:** 28 files
**Effort:** 40+ hours
**Risk:** HIGH (touches many files)

### 2.1 Current Structure

Runtime subsystem contains 28 files across multiple concerns:

| Concern | Files | Purpose |
|---------|-------|---------|
| Book context | 8 | BookContext, BookManager, BookParser, etc. |
| Chapter context | 6 | ChapterContext, ChapterManager, ChapterDocuments |
| Audio management | 3 | AudioBufferContext, AudioBufferManager |
| Artifact resolution | 2 | FileArtifactResolver, IArtifactResolver |
| Document slots | 4 | DocumentSlot, DocumentSlotOptions, adapters |
| Workspace | 2 | IWorkspace, WorkspaceChapterDiscovery |
| Interfaces | 3 | IBookManager, IChapterManager, IAudioBufferManager |

### 2.2 Proposed Split

```
Runtime/
├── Core/                    # Core runtime types
│   ├── DocumentSlot.cs
│   ├── DocumentSlotOptions.cs
│   ├── IDocumentSlotAdapter.cs
│   └── DelegateDocumentSlotAdapter.cs
├── Workspace/               # Workspace abstraction
│   ├── IWorkspace.cs
│   └── WorkspaceChapterDiscovery.cs
├── Book/                    # Book lifecycle (unchanged)
│   └── ... existing files
├── Chapter/                 # Chapter lifecycle (unchanged)
│   └── ... existing files
├── Audio/                   # Audio buffer lifecycle
│   └── ... existing files
└── Artifacts/               # Artifact resolution
    ├── FileArtifactResolver.cs
    └── IArtifactResolver.cs
```

### 2.3 Recommendation: DEFER

**Reasons to defer:**
1. No immediate pain point - subsystem works correctly
2. High effort (40+ hours) with limited ROI
3. Should wait until AlignmentService split is stable
4. May be better addressed as part of a larger architectural review

**When to reconsider:**
- Adding significant new Runtime features
- Performance issues related to context management
- New host types (beyond CLI, Web)

---

## 3. Over-Abstraction Removal

### 3.1 Remove IAudioService (DONE in PRUNING-PLAN.md)

**Issue ID:** AUD-005
**Status:** Covered in PRUNING-PLAN.md Phase 1.4

### 3.2 Interface Decision Matrix

**Issue IDs:** AUD-015, AUD-016, AUD-017, AUD-018

| Interface | Implementation | DI Registered | Mocked in Tests | Decision |
|-----------|---------------|---------------|-----------------|----------|
| `IMfaService` | `MfaService` | No | No | **REMOVE** |
| `IBookParser` | `BookParser` | Yes | No | **KEEP** (DI value) |
| `IBookIndexer` | `BookIndexer` | Yes | No | **KEEP** (DI value) |
| `IBookCache` | `BookCache` | Yes | No | **KEEP** (DI value) |

### 3.3 IMfaService Removal

**Current state:**
- Interface defined in `MfaService.cs`
- Single implementation `MfaService`
- Not registered in DI container
- Not used in tests
- `MfaWorkflow` uses concrete `MfaService` directly

**Migration:**
1. Remove `IMfaService` interface definition
2. Update any callers to use `MfaService` directly
3. No DI changes needed (not registered)

**Effort:** 30 minutes

### 3.4 IBook* Interfaces - Keep

**Rationale for keeping:**
- All three interfaces ARE registered in DI
- Enable swapping implementations if needed
- Follow established pattern in codebase
- Low maintenance burden

**No action required.**

---

## Summary: Candidate Action Matrix

| Candidate | Impact | Effort | Risk | Recommendation |
|-----------|--------|--------|------|----------------|
| AlignmentService Decomposition | HIGH | 16-24h | MEDIUM | **DO NOW** - Major testability improvement |
| Runtime Decomposition | LOW | 40h+ | HIGH | **DEFER** - No immediate benefit |
| IAudioService Removal | LOW | 30min | NONE | **DO NOW** - Dead code (in PRUNING-PLAN.md) |
| IMfaService Removal | LOW | 30min | LOW | **DO NOW** - Unnecessary abstraction |
| IBook* Interface Removal | LOW | 2h | LOW | **SKIP** - Keep for DI value |

---

## Recommended Execution Order

### Phase 4.1: Quick Cleanup (1-2 hours)
1. Remove IAudioService + AudioService (PRUNING-PLAN.md)
2. Remove IMfaService interface

### Phase 4.2: Consolidation (7-12 hours)
1. Extract ChapterLabelResolver (CONSOLIDATION-PLAN.md)
2. Consolidate ASR buffer preparation (CONSOLIDATION-PLAN.md)
3. Relocate MFA artifacts (CONSOLIDATION-PLAN.md)

### Phase 4.3: Major Refactoring (16-24 hours, if prioritized)
1. Create AnchorComputeService
2. Create TranscriptIndexService
3. Create TranscriptHydrationService
4. Create AlignmentService facade
5. Update DI and tests

### Deferred
- Runtime subsystem decomposition (future milestone)

---

*Generated: 2025-12-30*
*Source: Phase 3 artifacts (RESPONSIBILITY-MAP.md, ISSUES-CATALOGUE.md, CORE-SUBSYSTEMS.md)*
