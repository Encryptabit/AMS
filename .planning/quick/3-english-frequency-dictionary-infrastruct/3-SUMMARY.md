---
phase: quick-3
plan: 01
subsystem: book-indexing
tags: [frequency-dictionary, proper-nouns, book-indexer, asr-prompting]
dependency_graph:
  requires: []
  provides: [EnglishFrequencyDictionary, SectionRange.ProperNouns, bracket-phrase-extraction]
  affects: [BookIndexer, BookModels, AsrProcessor-future]
tech_stack:
  added: [System.Collections.Frozen.FrozenDictionary]
  patterns: [embedded-resource, static-singleton, bracket-state-machine]
key_files:
  created:
    - host/Ams.Core/Runtime/Book/EnglishFrequencyDictionary.cs
    - host/Ams.Core/Resources/english-frequency-50k.txt
    - host/Ams.Tests/BookIndexerProperNounTests.cs
  modified:
    - host/Ams.Core/Runtime/Book/BookModels.cs
    - host/Ams.Core/Runtime/Book/BookIndexer.cs
    - host/Ams.Core/Ams.Core.csproj
decisions:
  - FrozenDictionary<string,int> for O(1) lookup with zero ongoing allocation
  - Bracket safety valve at 8 tokens to prevent unbounded accumulation
  - Hyphenated words flagged only when ALL components are rare/unknown
  - Possessive stripping ('s) before frequency lookup
metrics:
  duration: 20m
  completed: 2026-03-02
---

# Phase quick-3 Plan 01: English Frequency Dictionary + Proper Noun Extraction Summary

FrozenDictionary-backed 50k English word frequency lookup with bracket phrase state machine and per-section proper noun extraction in BookIndexer.

## What Was Built

### EnglishFrequencyDictionary (static singleton)
- 50,000 English words loaded from embedded resource via `Assembly.GetManifestResourceStream`
- `Lazy<FrozenDictionary<string, int>>` for thread-safe, zero-allocation O(1) lookups
- Public API: `GetRank(word)` returns 1-based rank or -1, `IsRareOrUnknown(word)` for filtering, `Count` property

### SectionRange.ProperNouns
- Optional `string[]? ProperNouns` added as last parameter with `null` default
- JSON serialized as `properNouns` -- omitted when null via existing serialization settings
- All existing call sites unaffected (positional args unchanged)

### BookIndexer.Process Proper Noun Extraction
- **Bracket phrase state machine**: Tracks `[...]` and `<...>` phrases. Accumulates tokens, joins with space on close. 8-token safety valve abandons unclosed brackets.
- **Frequency filtering**: Non-bracketed lexical tokens checked against dictionary. Rare/unknown words added to per-section HashSet.
- **Hyphenated handling**: Split on `-`, flag full token only if ALL components are rare.
- **No double-hits**: Tokens inside brackets skip frequency check entirely.
- **Section scoping**: ProperNouns populated at both section-close sites, HashSet reset per section.

### Vault Improvements Applied
- `CollectLexicalTokens`: Return type changed from `IEnumerable<string>` to `IReadOnlySet<string>` (avoids re-enumeration, caller already compatible via IEnumerable covariance)
- `NormalizeTokenSurface`: Merged `.Trim()` + `TrimOuterQuotes()` into single-pass scan from both ends, eliminating intermediate string allocation

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| 1 | d1b16f6 | Frequency dictionary infrastructure + SectionRange model update |
| 2 (RED) | 3559c00 | Failing tests for bracket extraction and frequency filtering |
| 2 (GREEN) | 2116335 | Implement bracket phrase tracking and frequency filtering |

## Test Results

- 9 new `BookIndexerProperNounTests` -- all pass
- 115 existing tests pass (3 pre-existing ChapterLabelResolver failures on Linux path separators -- unrelated)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Missing common words in frequency dictionary**
- **Found during:** Task 2 GREEN phase
- **Issue:** Generated frequency list lacked common past-tense forms (sat, mat, appeared) and other inflected forms, causing false-positive proper noun detections
- **Fix:** Added ~118 missing common English words (past tenses, past participles, common verbs) to embedded resource
- **Files modified:** host/Ams.Core/Resources/english-frequency-50k.txt
- **Commit:** 2116335

## Self-Check: PASSED

All 3 created files exist. All 3 commit hashes verified in git log.
