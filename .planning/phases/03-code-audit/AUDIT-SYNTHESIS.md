# Phase 3: Code Audit Synthesis

**Complete audit of AMS codebase: 31 issues identified, 7 high-priority items, ~650 lines of removable code**

Generated: 2025-12-28

---

## Executive Summary

### Key Metrics

| Metric | Value | Assessment |
|--------|-------|------------|
| Total Issues | 31 | Manageable backlog |
| Critical Issues | 0 | No blockers |
| High-Priority Issues | 7 | Phase 4 focus |
| Dead Code (files) | 10-13 | ~0.5% of codebase |
| Dead Code (methods) | 9 | ~185 lines |
| Dead Code (total lines) | ~650 | Clean removal possible |
| Over-abstractions | 5 | Minor cleanup needed |
| Architecture Health | 6.8/10 | FAIR-GOOD |
| Test Pass Rate | 95.7% | 44/46 tests pass |

### Top 3 Findings

1. **AlignmentService is a god class** (681 lines, 4 responsibilities) - The largest architectural issue requiring attention. Splitting this class would significantly improve testability and maintainability.

2. **~650 lines of confirmed dead code** - Includes unused demo files, empty placeholders, and methods with zero callers. All can be safely removed with no impact on functionality.

3. **One broken analysis tool** - OverlayTest references a removed method and fails to build. Should be deleted immediately.

### Codebase Health Assessment

The AMS codebase demonstrates **solid architectural foundations** with a clean 3-layer project structure (Hosts -> Core -> Native) and well-organized subsystems. The issues identified are typical of an organically grown codebase and are all addressable without major architectural changes.

**Strengths:**
- Clean project-level architecture with no circular dependencies
- Well-defined subsystem boundaries (ASR, Alignment, MFA, Audio, Book, Prosody)
- Functional test suite with 95.7% pass rate (44/46 tests)
- Active projects build cleanly with zero warnings
- Web stack is functional and properly integrated with Core

**Areas for Improvement:**
- AlignmentService needs decomposition
- Dead code should be removed
- Test coverage could be expanded (currently ~6.5%)
- Some scattered logic needs consolidation

**Risk Level:** LOW - All issues are manageable and can be addressed incrementally.

---

## Dead Code Analysis Summary

**From DEAD-CODE.md**

### Totals

| Category | Count | Lines |
|----------|-------|-------|
| Files (HIGH confidence) | 10 | ~397 |
| Files (MEDIUM confidence) | 3 | ~83 |
| Methods | 9 | ~185 |
| **Total Removable** | 22 items | ~650 |

### Highest-Impact Removals

1. **SentenceTimelineBuilder.cs** (182 lines) - Zero callers, superseded functionality
2. **DspDemoRunner.cs** (141 lines) - Demo code with no production use
3. **Unused AudioProcessor methods** (~185 lines) - 9 methods never called
4. **Empty Wn*.cs placeholders** (32 lines) - Scaffolded but never implemented

### Risk Assessment for Removal

**LOW RISK** - All identified dead code has been verified through:
- Grep search for callers
- Interface implementation checks
- Event handler verification
- Build dependency analysis

No breaking changes expected from removals.

---

## Responsibility Analysis Summary

**From RESPONSIBILITY-MAP.md**

### Subsystem Health

| Subsystem | Files | Health | Notes |
|-----------|-------|--------|-------|
| Pipeline | 10 | Healthy | Clean command pattern |
| ASR | 8 | Healthy | Good isolation |
| Alignment | 15 | Warning | God class issue |
| MFA | 8 | Healthy | Well-organized |
| Audio | 15 | Healthy | Some dead code |
| Book/Doc | 12 | Healthy | Clean structure |
| Prosody | 9 | Healthy | Needs standardization |
| Validation | 4 | Healthy | Scattered files |
| Runtime | 28 | Warning | Oversized |
| Infrastructure | 16 | Healthy | Proper utilities |

### Key Consolidation Opportunities

1. **Extract ChapterLabelResolver** - Eliminate section resolution duplication between ChapterContext and AlignmentService
2. **Consolidate ASR buffer preparation** - Single implementation for mono downmix
3. **Relocate MFA artifacts** - Move to Application/Mfa/ for cohesion
4. **Standardize validation files** - Consolidate into dedicated folder

### Architecture Health Score: 6.8/10

| Dimension | Score | Notes |
|-----------|-------|-------|
| Cohesion | 6/10 | Runtime oversized, AlignmentService bloated |
| Coupling | 7/10 | Clean layer separation |
| Abstraction | 7/10 | Minor over-abstraction |
| Organization | 6/10 | Some scattered logic |
| Dead Code | 8/10 | Only 0.5% dead code |

---

## Project Portfolio Summary

**From PROJECT-AUDIT.md**

### Active Projects (4) - All Healthy

| Project | Status | Build | Notes |
|---------|--------|-------|-------|
| Ams.Core | Active | Clean | 96 source files, core logic |
| Ams.Cli | Active | Clean | 22 source files, CLI + REPL |
| Ams.Dsp.Native | Active | Clean | Native interop layer |
| Ams.Dsp.Native.Tests | Active | Clean | DSP tests |

### Non-Active Projects (8)

| Project | Status | Build | Recommendation |
|---------|--------|-------|----------------|
| Ams.Tests | Stale | 44/46 pass | Update (fix FFmpeg tests) |
| Ams.UI.Avalonia | Dormant | Clean | Archive (empty skeleton) |
| Ams.Web.Api | Nascent | Clean | Keep (functional API) |
| Ams.Web.Shared | Nascent | Clean | Keep (remove Class1.cs) |
| Ams.Web.Client | Nascent | 1 warning | Keep (functional UI) |
| Ams.Web | Nascent | Clean | Keep (server host) |
| OverlayTest | Analysis | FAIL | **Remove** (broken) |
| InspectDocX | Analysis | Clean | Archive |

### Portfolio Simplification

**Immediate Actions:**
1. Delete OverlayTest (broken, references removed method)
2. Archive Ams.UI.Avalonia (no functionality)
3. Archive InspectDocX (one-time utility)

**Result:** 11 projects -> 8 projects (or 9 if keeping archives)

---

## Phase 4 Readiness Assessment

### Prerequisites Met

| Prerequisite | Status | Notes |
|--------------|--------|-------|
| Dead code catalogued | COMPLETE | 22 items, ~650 lines |
| Responsibilities mapped | COMPLETE | 10 subsystems analyzed |
| Projects audited | COMPLETE | 8 non-active projects verified |
| Issues prioritized | COMPLETE | 31 issues with severity/effort |
| Health score calculated | COMPLETE | 6.8/10 (FAIR-GOOD) |

### Phase 4 Focus Areas

**Recommended prioritization for Phase 4 (Recommendations):**

1. **Quick Wins Package** (70 minutes total)
   - Delete OverlayTest, Class1.cs, Wn*.cs, ManifestV2.cs, IAudioService
   - ~90 lines of dead code removed with minimal effort

2. **Test Suite Stabilization** (4-6 hours)
   - Fix FFmpeg filter tests (AUD-007)
   - Restore 100% test pass rate

3. **Immediate Dead Code Removal** (2-4 hours)
   - Remove DspDemoRunner.cs, SentenceTimelineBuilder.cs
   - Remove unused AudioProcessor methods
   - ~500 additional lines removed

4. **Scattered Logic Consolidation** (8-12 hours)
   - Extract ChapterLabelResolver utility
   - Consolidate ASR buffer preparation
   - Relocate MFA artifacts

5. **AlignmentService Decomposition** (16-24 hours, if prioritized)
   - Split into focused service classes
   - Improve testability significantly

### Questions for Phase 4

1. **Should AlignmentService decomposition be prioritized?** It's high-impact but high-effort. Decision depends on whether new alignment features are planned.

2. **Should questionable interfaces be removed?** IMfaService, IBookParser, IBookIndexer, IBookCache have single implementations. Need to verify if they're needed for testing mocks.

3. **What's the future of the Web stack?** Ams.Web.* projects are functional but nascent. Decide: invest further, maintain as-is, or archive.

4. **Archive strategy?** Should archived projects (UI.Avalonia, InspectDocX) be moved to archive/ folder or removed entirely?

---

## Metrics Dashboard

### Codebase Statistics

| Metric | Phase 1 | Phase 3 | Change |
|--------|---------|---------|--------|
| Total files | 146 | 146 | - |
| Source files | 137 | 137 | - |
| Test files | 9 | 9 | - |
| Dead code % | Unknown | ~0.5% | Measured |
| Test coverage | ~6.5% | ~6.5% | - |
| Build warnings | Unknown | 1 | Measured |
| Test pass rate | Unknown | 95.7% | Measured |

### Architecture Health Trend

| Dimension | Score |
|-----------|-------|
| Cohesion | 6/10 |
| Coupling | 7/10 |
| Abstraction | 7/10 |
| Organization | 6/10 |
| Dead Code | 8/10 |
| **Overall** | **6.8/10** |

### Issue Distribution

```
Critical   [0]
High       [7]  #######
Medium     [15] ###############
Low        [9]  #########
```

---

## Appendix: Complete Deliverables

### Phase 3 Plan 1 (03-01): Dead Code Analysis

| Deliverable | Status | Description |
|-------------|--------|-------------|
| ORPHANED-FILES.md | Complete | Orphaned file identification |
| UNUSED-METHODS.md | Complete | Unused method scan |
| DEAD-CODE.md | Complete | Consolidated dead code catalogue |

### Phase 3 Plan 2 (03-02): Responsibility Analysis

| Deliverable | Status | Description |
|-------------|--------|-------------|
| CORE-SUBSYSTEMS.md | Complete | Subsystem categorization |
| SCATTERED-LOGIC.md | Complete | Scattered responsibility analysis |
| RESPONSIBILITY-MAP.md | Complete | Architecture health assessment |

### Phase 3 Plan 3 (03-03): Project Audit & Synthesis

| Deliverable | Status | Description |
|-------------|--------|-------------|
| PROJECT-AUDIT.md | Complete | Non-active project verification |
| ISSUES-CATALOGUE.md | Complete | Consolidated issues with IDs |
| AUDIT-SYNTHESIS.md | Complete | Executive summary and synthesis |

---

## Conclusion

Phase 3 Code Audit has thoroughly analyzed the AMS codebase and produced a clear picture of its health. The codebase is in **FAIR-GOOD condition (6.8/10)** with no critical issues blocking development.

**Key Takeaways:**

1. **The core architecture is sound.** The 3-layer project structure and 10 subsystem organization provide a solid foundation.

2. **Dead code is minimal and safe to remove.** ~650 lines (~0.5%) can be removed with low risk.

3. **One major refactoring opportunity exists.** AlignmentService decomposition would significantly improve testability.

4. **Project portfolio can be simplified.** 3 projects should be archived/removed (OverlayTest, UI.Avalonia, InspectDocX).

5. **Quick wins are available.** 70 minutes of effort removes 90 lines of dead code and cleans up the codebase.

**Phase 3: Code Audit - COMPLETE**

Ready for Phase 4: Recommendations

---

*Phase 3 Complete: 2025-12-28*
