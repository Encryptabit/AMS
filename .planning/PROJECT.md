# AMS Codebase Audit & Refactoring

## Current State (Updated: 2025-12-30)

**Shipped:** v1.0 AMS Codebase Audit (2025-12-30)
**Status:** Analysis Complete
**Codebase:** 146 C# files, 11 projects, ~18,500 LOC, .NET 9.0

### Audit Results
- **Health Score:** 6.8/10 (FAIR-GOOD)
- **Dead Code:** ~650 lines identified (10 orphaned files, 9 unused methods)
- **Issues Catalogued:** 31 (7 high, 15 medium, 9 low)
- **Pipeline:** 7-stage flow fully documented
- **Architecture:** Clean 3-layer confirmed, Core as hub (6 dependents)

### Next Steps
45 actions prioritized in ACTION-LIST.md with ~55 hours total effort.
Health improvement path: 6.8 -> 8.0/10.

## Next Milestone Goals

**Vision:** Execute prioritized refactoring to improve codebase health from 6.8 to 8.0

**Motivation:**
- Dead code removal will reduce cognitive load
- AlignmentService decomposition enables easier testing
- Scattered responsibility consolidation improves maintainability

**Scope (v1.1 Refactoring):**
- Immediate dead code deletion (~2 hours)
- AlignmentService decomposition (16-24 hours)
- Interface cleanup (IMfaService removal)
- Test suite stabilization

---

<details>
<summary>Original Vision (v1.0 - Archived for reference)</summary>

## Vision

This is a comprehensive audit and refactoring initiative for the Audio Management System (AMS) - a CLI and core library for audio processing, ASR, forced alignment, and audiobook mastering. The codebase has grown organically over time, and the author can no longer confidently hold the entire system in their head. Documentation is outdated. The pipeline order is uncertain. Dead code may lurk. Abstractions may over-complicate. Responsibilities may be scattered.

The goal is to achieve **complete, ground-truth understanding** of every line of code in the solution - what it does, why it exists, and whether it should continue to exist. This understanding will inform ruthless pruning, thoughtful consolidation, and architectural clarity. The result: a lean, well-organized codebase where the pipeline flow is obvious, responsibilities are clear, and nothing exists without purpose.

## Problem

The codebase has grown beyond what the author can comfortably reason about:

- **Uncertainty about the pipeline**: Even the core flow (ASR -> something -> MFA -> Merge) isn't crystal clear. Steps like book indexing and ASR response indexing happen somewhere, but their exact order and necessity is fuzzy.
- **Dead code uncertainty**: Not sure what's actually used vs orphaned code from abandoned approaches.
- **Scattered responsibilities**: Logic spread across files that should be together.
- **Too many abstractions**: Interfaces and services that obscure rather than clarify.
- **Outdated documentation**: All existing docs are stale and should be ignored.
- **Missing visibility**: The FFmpeg P/Invoke code (using reflection, unsafe blocks) wasn't captured by the call graph generator.

## Success Criteria

How we know this worked:

- [x] Complete call graph exists for every file (including FFmpeg/P/Invoke code manually documented)
- [x] Pipeline flow is documented with precise step order and data flow
- [x] Every file has a clear purpose documented
- [x] Dead code identified and catalogued
- [x] Consolidation opportunities identified
- [x] Architecture map shows clean module boundaries
- [x] Author can confidently explain any part of the codebase

## Scope

### Building

- Complete analysis report covering every C# file in the solution
- Method-level call graphs (regenerated fresh for the whole solution)
- Module dependency map
- Pipeline flow documentation (exact step order)
- Dead code inventory
- Consolidation recommendations
- Architecture clarity recommendations
- Manual documentation of FFmpeg P/Invoke code (missed by call graph generator)

### Not Building

- No actual code changes in this phase - analysis first, approval required before any pruning
- No new features
- No new dependencies
- No test updates (deferred until after reorganization)
- Not touching Zig code (dormant, future exercise)

## Context

### Project Breakdown

| Project | Status | Notes |
|---------|--------|-------|
| Ams.Cli | Active | Command-line interface, primary entry point |
| Ams.Core | Active | Core library with pipeline, alignment, FFmpeg integration |
| Ams.Dsp.Native | ? | DSP native interop - status TBD |
| Ams.Tests | Stale | Tests pass but coverage is outdated |
| Ams.UI.Avalonia | Dormant | Keep for later, not actively used |
| Ams.Web | Nascent | Started to replace tools/validation-viewer, paused until codebase is compact |

### Existing Assets

- **Call graphs in D:/Notes/**: ~140 markdown files with file-level call graphs covering most of the codebase
- **178 C# files** across the solution
- **FFmpeg P/Invoke code** in Ams.Core - uses reflection and unsafe blocks, not captured by call graph generator

### Core Pipeline (nominal)

ASR -> MFA -> Merge - but there are additional steps (book indexing, ASR response indexing) whose exact order and role needs clarification.

## Constraints

- **Pipeline must keep working**: The ASR -> alignment -> MFA -> merge flow is the core product and must never break
- **No new dependencies**: Don't introduce new libraries or tools
- **Analysis before action**: Document everything before touching any code

## Decisions Made

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Existing call graphs | Use D:/Notes as foundation | Already generated, saves time |
| New method-level graphs | Regenerate for whole solution | Need complete coverage |
| FFmpeg code | Document manually | Reflection/unsafe blocks not captured by generator |
| UI.Avalonia | Keep dormant | Has future value, not currently blocking |
| Tests | Defer updates | Fix after reorganization is complete |
| Approach | Analysis report first | Understand before modifying |

## Open Questions

Things to figure out during analysis:

- [x] What is the exact pipeline step order?
- [x] What does Ams.Dsp.Native contain and is it active?
- [x] Which abstractions are over-engineered vs genuinely useful?
- [x] What code is truly dead vs dormant-but-useful?
- [x] Where are the FFmpeg P/Invoke entry points in Ams.Core?
- [x] What's in tools/validation-viewer that Ams.Web was meant to replace?

---
*Initialized: 2025-12-28*

</details>
