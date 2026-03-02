---
phase: quick-4
plan: 01
subsystem: asr
tags: [whisper, prompt, proper-nouns, vocabulary-biasing]
dependency-graph:
  requires: [quick-3-01]
  provides: [asr-prompt-wiring]
  affects: [asr-pipeline, whisper-decoding]
tech-stack:
  added: []
  patterns: [conditional-builder-wiring, section-scoped-vocabulary]
key-files:
  created: []
  modified:
    - host/Ams.Core/Processors/AsrProcessor.cs
    - host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs
    - host/Ams.Core/Runtime/Book/BookModels.cs
decisions:
  - Comma-separated proper nouns as Whisper prompt (primes decoder vocabulary without forming sentences)
  - Section lookup via BookStartWord match (O(n) scan over sections array, typically small)
  - Debug-level logging with truncation at 200 chars for long prompt strings
metrics:
  duration: 169s
  completed: 2026-03-02T02:43:45Z
  tasks: 2/2
  files-modified: 3
---

# Phase quick-4 Plan 01: Wire WithPrompt in AsrProcessor Summary

Whisper initial prompt wired from BookIndex section-scoped ProperNouns through AsrOptions to ConfigureBuilder.WithPrompt, biasing ASR decoding toward domain vocabulary (fantasy names, rare terms).

## What Changed

### Task 1: Add Prompt to AsrOptions and wire WithPrompt in ConfigureBuilder
- **Commit:** fcd491f
- Added `string? Prompt = null` as the last parameter to the `AsrOptions` positional record (backward compatible default)
- Added conditional `builder.WithPrompt(options.Prompt)` call in `ConfigureBuilder` after the beam/greedy strategy block
- Added `string[]? ProperNouns = null` to `SectionRange` record (shared change with quick-3 for clean merge)

### Task 2: Build prompt from BookIndex ProperNouns in GenerateTranscriptCommand
- **Commit:** 52b37dd
- Added `BuildAsrPrompt(ChapterContext)` private static helper that resolves the chapter's section from BookIndex by matching `Descriptor.BookStartWord`
- Extracts `ProperNouns` from the matched `SectionRange` and joins them with commas
- Wired prompt resolution before `AsrOptions` construction in `RunWhisperAsync`
- Added debug log showing term count and truncated prompt content when applied
- Gracefully returns null when: no BookIndex, no sections, no BookStartWord, no matching section, or no ProperNouns

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added ProperNouns to SectionRange for compilation**
- **Found during:** Task 1
- **Issue:** Quick-3 (running in parallel) adds ProperNouns to SectionRange, but this worktree doesn't have that change yet
- **Fix:** Added `string[]? ProperNouns = null` with `[property: JsonPropertyName("properNouns")]` to SectionRange as the last parameter with default null, matching the exact quick-3 change for clean merge
- **Files modified:** host/Ams.Core/Runtime/Book/BookModels.cs
- **Commit:** fcd491f

**2. [Rule 3 - Blocking] Added using for Ams.Core.Runtime.Book namespace**
- **Found during:** Task 2
- **Issue:** GenerateTranscriptCommand needed access to SectionRange type for BuildAsrPrompt
- **Fix:** Added `using Ams.Core.Runtime.Book;` to imports
- **Files modified:** host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs
- **Commit:** 52b37dd

## Verification Results

- Full Ams.Core build: 0 warnings, 0 errors
- Test suite: 106 passed, 3 failed (pre-existing ChapterLabelResolver platform failures, unrelated)
- AsrOptions has `Prompt` as final parameter with null default
- ConfigureBuilder calls WithPrompt conditionally
- BuildAsrPrompt correctly resolves section by BookStartWord
- Null safety confirmed for all degradation paths

## Pre-existing Test Failures (Out of Scope)

3 tests in `ChapterLabelResolverTests` fail on Linux due to Windows path assumptions. These failures exist on the base branch without any of these changes applied. Logged to deferred-items if not already tracked.

## Self-Check: PASSED

- All 3 modified files exist on disk
- SUMMARY.md created
- Commit fcd491f verified in git log
- Commit 52b37dd verified in git log
