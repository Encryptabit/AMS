# Roadmap: AMS Codebase Audit & Refactoring

## Overview

A systematic audit of the Audio Management System codebase to achieve complete understanding of every file, trace the pipeline flow, identify dead code and over-abstraction, and produce actionable refactoring recommendations. Analysis first, code changes only after approval.

## Domain Expertise

None (internal codebase audit - no external domain expertise applicable)

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

- [x] **Phase 1: Discovery** - Map complete codebase structure and regenerate call graphs
- [x] **Phase 2: Pipeline Analysis** - Document exact pipeline flow with step order and data flow
- [x] **Phase 3: Code Audit** - Identify dead code, scattered responsibilities, over-abstraction
- [x] **Phase 4: Recommendations** - Consolidate findings into actionable refactoring plan

## Phase Details

### Phase 1: Discovery
**Goal**: Complete inventory of all 277 C# files with purpose annotations; call graph coverage analysis; manual documentation of FFmpeg P/Invoke code; module dependency map
**Depends on**: Nothing (first phase)
**Research**: Unlikely (file enumeration, existing call graph review)
**Plans**: 4 plans created
- 01-01: File Inventory & Project Structure (3 tasks)
- 01-02: Call Graph Analysis (3 tasks)
- 01-03: FFmpeg/P/Invoke Documentation (3 tasks)
- 01-04: Module Dependency Map (3 tasks)

Key deliverables:
- File inventory with purpose annotations per file
- Method-level call graphs (regenerated fresh)
- FFmpeg/P/Invoke documentation (manual, since reflection/unsafe missed by generator)
- Module dependency map

### Phase 2: Pipeline Analysis
**Goal**: Crystal-clear documentation of ASR → alignment → MFA → merge flow with exact step order, data inputs/outputs, and intermediate artifacts
**Depends on**: Phase 1
**Research**: Unlikely (tracing existing code paths)
**Plans**: 3 plans created
- 02-01: Pipeline Orchestration Flow (3 tasks)
- 02-02: Data Flow & Artifacts (3 tasks)
- 02-03: Indexing Clarification (3 tasks)

Key deliverables:
- Pipeline step order documentation (precise sequence)
- Data flow diagram (what goes in/out of each step)
- Artifact inventory (all intermediate files produced)
- Book indexing and ASR response indexing clarification

### Phase 3: Code Audit
**Goal**: Systematic review of every file to identify dead code, scattered responsibilities, and over-engineered abstractions
**Depends on**: Phase 2
**Research**: Unlikely (internal code analysis)
**Plans**: 3 plans created
- 03-01: Dead Code Inventory (3 tasks)
- 03-02: Responsibility Analysis (3 tasks)
- 03-03: Project Audit & Synthesis (3 tasks)

Key deliverables:
- Dead code inventory (orphaned files, unused methods)
- Responsibility scatter map (logic that should be consolidated)
- Over-abstraction catalogue (interfaces/services that obscure rather than clarify)
- Per-project status verification (Ams.Dsp.Native, UI.Avalonia, Ams.Web)

### Phase 4: Recommendations
**Goal**: Synthesize all findings into prioritized, actionable refactoring recommendations with clear module boundaries
**Depends on**: Phase 3
**Research**: Unlikely (synthesizing prior phase findings)
**Plans**: 2 plans created
- 04-01: Pruning & Consolidation Plans (3 tasks)
- 04-02: Architecture Map & Action Sequencing (3 tasks)

Key deliverables:
- Consolidation recommendations (what to merge)
- Pruning recommendations (what to delete)
- Architecture clarity map (proposed module boundaries)
- Prioritized action list (what to do first)

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3 → 4

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Discovery | 4/4 | Complete | 2025-12-28 |
| 2. Pipeline Analysis | 3/3 | Complete | 2025-12-28 |
| 3. Code Audit | 3/3 | Complete | 2025-12-29 |
| 4. Recommendations | 2/2 | Complete | 2025-12-30 |
