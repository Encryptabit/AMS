# Treat / MFA Recovery Plan

Three-step plan for closing the title-body gap stretching observed in Chapters 3 / 8 / 38 of `Neighborhood Necromancer 1`, plus follow-ups. Steps are ordered smallest-blast-radius first; A and C plug into each other.

## Status snapshot

| Step | Description | State |
|------|-------------|-------|
| **B** | Defensive snap of hydrate-derived title/body boundaries around contained silences | **Done & verified.** |
| **A** | Tiered recovery (None → AlternateModel → Promptless) + `AmsAsrModel` whitelist | **Done & verified.** |
| **C** | Chunk-scoped recovery — re-ASR only the chunks MFA flagged, merge back into existing `asr.json` | **Done & verified.** |
| **D** | Humanizer integration + natural ordering audit — replace hand-rolled `NumberToWords` with Humanizer, add ordinal-words pass, and centralize human-facing chapter/WAV natural ordering. | **Implemented & focused-test verified. Full suite still has unrelated proof gesture failure.** |

**First action on resume:** review the Step D diff if desired, then address the pre-existing `ProofGestureSelectionContractTests.LongPress_…` failure or continue to the next recovery objective.

---

## Background context (necessary for the next session)

### The bug

`qc analyze` reported `Gap=2.82 s` (target 1.5 s) on Chapter 3 / 8 / 38 in the Neighborhood Necromancer 1 book. After a long investigation:

- ASR's word timings were tight (`"3"` ended at ~1.03 s; "A" started at ~3.92 s).
- The book index had `"3"` with phoneme `θ ɹ iː` (correct).
- The MFA TextGrid I inspected initially showed `"iii"` at 4.00–4.06 s, which led me down a Roman-numeral-normalization rabbit hole. That TextGrid was a stale artifact from an earlier pipeline run with different model settings — **not** the actual root cause for the latest run.
- The actual root cause: MFA's chunked aligner stretches the trailing word of a *sparse-text chunk* to fill the chunk's audio. When chunk-0's lab text doesn't include enough body words after the title, the title's last word's `End` slides past the natural silence into where the body actually starts. Hydrate inherits the stretch via `ApplySentenceTimings`'s `Math.Max(end)`. Treat's `TryFindBoundariesFromHydrate` then reads `titleSentence.endSec` and extracts a title segment that swallows the silence; a 1.5 s roomtone gap gets appended on top → 2.82 s effective gap in qc.

### The merger is correct

`MfaTimingMerger.cs` (`ApplySentenceTimings` lines 304–329) already constructs sentence timings as the tight envelope of word timings (`min(start)`, `max(end)`). Don't re-canonicalize there — the stretch is at the **word** level, not sentence-aggregation.

### User's first-try-with-LargeV3Turbo evidence

When the user reran with `large-v3-turbo` + `--dtw-timestamps`, everything worked first try (no MFA recovery, no title-body issue). This is what motivates **A** — different acoustic model = different chunk-text/word-timing patterns = MFA can align cleanly without the recovery dance.

### What was confirmed unnecessary

- **Roman numeral normalization** in lab text. Book uses Arabic numerals; the "iii" was a stale TextGrid. User explicitly **dropped this from the roadmap** — risk of false positives in litRPG content (item names, system text) is too high for the questionable upside. Not coming back unless there's evidence of actual Roman numerals causing problems.
- **Sentence-timing canonicalization in the merger.** Already correct.

### What was deferred

- **Humanizer integration** — promoted to the next objective; see "Step D" below.
- **Abbreviation expansion** (`Dr.` → `Doctor` vs `Drive`). Deferred indefinitely — no robust programmatic disambiguation without semantic context.

---

## Step D — Humanizer integration + natural ordering audit

### Goal

Replace the hand-rolled `NumberToWords` in `PronunciationHelper.cs` with Humanizer's `ToWords()` so proofing-side comparisons handle `twenty-one` ≡ `21`, plus add an ordinal-words pass (`1st` ≡ `first`). Also review human-facing chapter/WAV sorting so natural numeric names sort as users expect (`Chapter 2` before `Chapter 10`). Touches comparison/lookup/order-display paths only — no audio side effects.

For .NET 10, numeric-aware ordering is not `StringComparer.NumericOrdering`; use a comparer built from `CompareOptions.NumericOrdering`, e.g. `StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.NumericOrdering | CompareOptions.IgnoreCase)` or a small shared wrapper around that. Apply this only to user-facing labels, stems, and filenames where natural ordering is intended. Keep `StringComparer.Ordinal` / `OrdinalIgnoreCase` for opaque IDs, operation IDs, artifact references, path keys, cache keys, and deterministic internal contracts.

### Scope

- Add `Humanizer` package reference to the project that owns `PronunciationHelper.cs` (verify which `.csproj` before adding).
- Replace `NumberToWords` body with `number.ToWords(CultureInfo.InvariantCulture)` (or `new CultureInfo("en-US")` if Humanizer's invariant fallback is lossy — check before defaulting).
- Add ordinals path: `number.ToOrdinalWords()` for `1st`/`2nd`/`3rd`/`Nth` patterns surfaced by the proofing comparator.
- Audit current callers of `NumberToWords` to confirm the contract Humanizer's output satisfies (hyphenation, capitalization, "and" usage for tens — `one hundred and one` vs `one hundred one`). Lock the cultural variant to whatever the existing tests expect.
- Audit current `OrderBy` / `ThenBy` usage over chapter stems, display titles, and WAV filenames. The existing duplicated `ChapterFileComparer` in core chapter discovery and the CLI REPL is the first target; evaluate replacing or centralizing it with the .NET 10 numeric comparer while preserving matched-book-index-first behavior.
- Add/keep a single project-local helper for "natural human label order" if more than one call site needs it, so future sorting does not drift back to ad hoc regex or plain ordinal string ordering.

### Out of scope

- Audio pipeline (ASR / MFA / hydrate / treat).
- Abbreviation expansion.
- Roman numerals (already dropped).
- Reordering opaque IDs, operation logs, ledger entries, artifact references, or any sort where ordinal/deterministic order is intentionally part of the contract.

### File touch points

- `host/Ams.Core/.../PronunciationHelper.cs` (exact path TBD — search `NumberToWords` usages first).
- The `.csproj` adjacent to `PronunciationHelper.cs` for the package reference.
- `ChapterDiscoveryService` and CLI REPL chapter scanning, which currently have duplicated numeric-aware `ChapterFileComparer` logic.
- Workstation chapter/pickup/proof surfaces that sort `ChapterStem` or WAV-backed chapter names for display.
- Tests covering `PronunciationHelper` / proofing comparison.
- Tests covering chapter/WAV natural ordering, especially `chapter 2` / `chapter 10` and equivalent filename stems.

### Verification

- Existing unit tests must stay green or be updated explicitly to match Humanizer's casing/hyphenation if it differs.
- Add a regression test for the original `twenty-one` ≠ `21` mismatch case.
- Add an ordinals test (`1st` ≡ `first`).
- Add a natural-ordering regression test showing `Chapter 2` sorts before `Chapter 10` / `chapter_2.wav` before `chapter_10.wav` at the shared helper or chapter discovery boundary.
- Sanity-check the failing-dataset proofing pass on Neighborhood Necromancer 1 chapters that previously surfaced number mismatches.

---

## Step B — Defensive snap (in review)

### What landed in the diff

- New private static helper in `host/Ams.Core/Audio/AudioTreatmentService.cs`:

  ```csharp
  private static SilenceInterval? FindLongestSilenceInWindow(
      double windowStart, double windowEnd, IReadOnlyList<SilenceInterval> silenceIntervals)
  ```

  Returns the longest silence overlapping the open window `(windowStart, windowEnd)`, or `null`. Excludes silences that abut either edge.

- `TryFindBoundariesFromHydrate` and `TryFindDecoratorTitleLayoutFromHydrate` now both take an `IReadOnlyList<SilenceInterval> silenceIntervals` parameter. After computing raw boundaries from hydrate, they call `FindLongestSilenceInWindow(titleStart, contentStart, silences)` (or the equivalent decorator window) and apply `Math.Min` / `Math.Max` clamps. Snap only ever tightens — boundaries already correct pass through.

- The two callers (`FindSpeechBoundaries`, `FindTreatmentLayout`) thread `silenceIntervals` through. `TreatChapterCoreAsync` already had the silence list — no new computation.

- 2 new tests in `host/Ams.Tests/Audio/AudioTreatmentServiceTests.cs`:
  - `FindSpeechBoundaries_SnapsStretchedHydrateTitleEndOutOfSilence` (regression).
  - `FindSpeechBoundaries_DoesNotMutateHydrateBoundariesAlreadyOutsideSilences` (non-regression).

### Diff metrics

`+177 / -4`, 2 files. Build clean, 19/19 AudioTreatmentService tests pass, 891/892 of full assembly (the one failure is `ProofGestureSelectionContractTests.LongPress_…`, unrelated WIP in pre-existing Pickups files).

### What's blocking

Reviewer requested changes on `rvw_3f932fa6a1b240b39546f94ec16a269c`. Verdict text not yet retrieved — pull it via `mcp__tandem-review__get_review_status` and `get_discussion`. Resubmission flow is `add_message` with `body` + `diffPath` (NOT `create_review` — that creates a fresh review and breaks the audit trail; the previous session already learned this).

---

## Step A — Tiered recovery + model whitelist

### Goal

Replace today's binary recovery (`promptlessRecoveryPass: bool`) with a graduated escalation that tries a less-invasive remediation first. The user's evidence: switching from `large-v3` to `large-v3-turbo` (with the same prompt and DTW flag) resolved the same chapter that promptless ASR had to wrestle with — preserving the proper-noun prompt is more valuable than disabling it as a first move.

### Tiering state machine

Introduce a `RecoveryTier` enum:

```csharp
public enum RecoveryTier
{
    None,            // first attempt, user's flags
    AlternateModel,  // re-ASR with the cross-pair model, prompt preserved
    Promptless       // re-ASR promptless (today's behavior), as last resort
}
```

`RunOutcome` in `host/Ams.Core/Application/Mfa/MfaWorkflow.cs` continues to signal `PromptlessAsrRetryRecommended` (the *workflow* doesn't need to know about tiers — that's an orchestrator concern). The `PipelineCommand` orchestrator inspects the previous tier to pick the next.

### Model whitelist

`GgmlType` is owned by `Whisper.net.Ggml` (third-party) — can't trim. Wrap with an in-tree `AmsAsrModel`:

```csharp
public enum AmsAsrModel { LargeV3, LargeV3Turbo }
```

Map `AmsAsrModel ↔ GgmlType` internally. CLI parser hard-fails on unrecognized model values (the user confirmed this is OK — Workstation UI uses a dropdown so this is dev-only friction). `AMS_WHISPER_MODEL_PATH` env var stays as the escape hatch for arbitrary `.bin` files.

### Fallback policy

Default cross-pair:

```
LargeV3       → LargeV3Turbo
LargeV3Turbo  → LargeV3
```

CLI override: `--fallback-model <name|none>`. `none` opts out of tier 2, going straight from tier 1 failure to Promptless. If a future user passes a model name we don't have a cross-pair for, treat it as opt-out (no AlternateModel tier, fall through to Promptless).

### Preserved across model switch

The user explicitly called out: when the alternate-model tier fires, **all other CLI flags must be preserved** — `--flash-attention`, `--dtw-timestamps`, `--language`, etc. The only thing that changes is the model. This is consistent with how today's promptless recovery already preserves these flags.

### Engine scoping

Tier 2 only applies for `AsrEngine.Whisper`. WhisperX paths skip directly from tier 1 to Promptless (its model loading is opaque to the in-process Whisper.NET cache).

### Telemetry

Log clearly per chapter what tier resolved it, including the alternate model name when relevant:

```
chapter Chapter 3 resolved at tier=AlternateModel(model=large-v3-turbo)
chapter Chapter 8 resolved at tier=Promptless
chapter Chapter 17 resolved at tier=None
```

### File touch points

- `host/Ams.Cli/Commands/PipelineCommand.cs` — recovery dispatch lives at three call sites (around lines 1419 single-progress, 1500 single-no-progress, 600+ batch). All three need the tier escalation.
- `host/Ams.Core/Application/Mfa/MfaWorkflow.cs` — keep `PromptlessAsrRetryRecommended`; orchestrator owns the tier.
- `host/Ams.Cli/Commands/PipelineCommand.cs` — new `--fallback-model` option, `RunPipelineAsync` signature gets a `RecoveryTier currentTier` parameter (replacing the `promptlessRecoveryPass: bool`).
- `host/Ams.Core/Asr/` — new `AmsAsrModel` enum + parser/validator. `AsrEngineConfig.cs` already has `DefaultModelType => GgmlType.LargeV3Turbo` — that becomes the source of truth for the cross-pair table.
- `host/Ams.Workstation.Server/Services/Prep/PrepRunSession.cs` and friends — Workstation also passes a model selection through. Verify the Workstation surface uses the new `AmsAsrModel` enum (likely a dropdown in `host/Ams.Workstation.Server/Components/Pages/Prep/Index.razor`).

### Cost notes

Switching models invalidates the Whisper factory cache (cache key includes `ModelPath` per `AsrProcessor.cs:19`). Tier 2 adds ~30 s model load + the re-ASR pass (~3 min for a long chapter). Acceptable; log the cost so users see it.

### Open design question for the next session

Should `RunOutcome` start carrying a list of *which chunks* were problematic? It already detects them — `MfaWorkflow.cs:254` aggregates per-utterance TextGrids and detects quality issues. If we surface them up to the orchestrator, that's a free leg up for **C**.

---

## Step C — Chunk-scoped recovery

### Goal

Today's promptless / alternate-model recovery re-ASRs the entire chapter. That's a sledgehammer when typically only 1–3 chunks have alignment issues. Chunk-scoped recovery: re-ASR *only* the flagged chunks (with the tier's chosen settings), splice their words into the existing `asr.json`, invalidate downstream chunked-MFA artifacts only for those chunks, re-run MFA on the patched corpus.

### Why it matters

- Cuts recovery cost from ~3 minutes to ~10–15 seconds per chunk.
- Eliminates collateral damage: chunks that aligned cleanly the first time keep their good word timings. Today's chapter-wide recovery can degrade good chunks (e.g., chunk-0's title region) for the sake of fixing a different bad chunk.
- Removes the load-bearing role that **B** plays for the title-body case. B becomes pure belt-and-suspenders once C lands.

### Plumbing

1. **Identify problematic chunks.** Already detected in `MfaWorkflow.cs:254`. Surface them up via `RunOutcome` (open question from A).
2. **Re-ASR scoped to chunk indices.** New entry point in `AsrService` / `AsrProcessor.cs` that takes a list of chunk-plan indices and re-transcribes only those. The chunk plan stays stable (it's fingerprinted on source audio + chunk policy in `ChunkPlanningService.cs`, not on ASR output).
3. **Splice into existing `asr.json`.** Replace word tokens whose timing falls within the re-ASR'd chunks' time ranges. Preserve everything else byte-identical.
4. **Invalidate per-chunk MFA artifacts.** Delete `utt-NNNN.lab` / `utt-NNNN.wav` / `utt-NNNN.TextGrid` for the affected chunks; rebuild via `MfaChunkCorpusBuilder` only for those.
5. **Re-run MFA align on the patched corpus.** Same MFA invocation, MFA itself doesn't care that some utterances are unchanged.
6. **Re-merge timings.** `MfaTimingMerger` runs on the full TextGrid set and produces a fresh hydrate.

### Critical invariant

The chunk plan must be stable across original-ASR and recovery-re-ASR. Today's chunk plan IS stable (it's regenerated only when the source audio fingerprint changes — see `ChunkPlanningService.IsValid`). Don't pass `--force` or anything that would regenerate the chunk plan during chunk-scoped recovery. Add a test for plan stability.

### File touch points

- `host/Ams.Core/Services/AsrService.cs` — new method `TranscribeChunksAsync(chapter, chunkIndices, options, ct)`.
- `host/Ams.Core/Processors/AsrProcessor.cs` — internal: re-use the existing per-chunk transcription path that's already there (`TranscribeBufferAsync`).
- `host/Ams.Core/Application/Mfa/MfaChunkCorpusBuilder.cs` — chunk-corpus rebuild scoped to specific indices.
- `host/Ams.Core/Application/Mfa/MfaWorkflow.cs` — `RunOutcome` carries problematic-chunk list (per A's open question).
- `host/Ams.Cli/Commands/PipelineCommand.cs` — orchestrator dispatches scoped re-ASR per tier.

### Plug-in to A

A's tiering state machine gains a "scoped" sibling:
- AlternateModel tier: re-ASR all flagged chunks with the cross-pair model. Prompt preserved.
- Promptless tier: re-ASR all flagged chunks promptless. (Or only if AlternateModel still left flagged chunks.)

C's surface area is independent of which tier triggered the re-ASR — it just defines *what* gets re-transcribed, not *how*.

---

## Sequencing notes for the next session

1. **Do not start A or C until B is approved.** B is the highest-confidence, smallest-blast-radius fix and it's already in review. Fix the requested changes, get it approved, commit + push.
2. **Re-run the failing dataset after B lands** to confirm Chapters 3 / 8 / 38 close to ~1.5 s gaps. If they don't, B's snap window logic needs revisiting before A/C work continues.
3. **A before C.** A defines the orchestrator state machine that C plugs into. Doing C first means rewriting it once A lands.
4. **Each step ships as its own review.** B → review → approve → commit → push → re-run dataset → A → review → … (this gives the user observable progress and minimizes context required per review).

---

## Reference: relevant code locations

| Concern | Path |
|---------|------|
| B's changes | `host/Ams.Core/Audio/AudioTreatmentService.cs`, `host/Ams.Tests/Audio/AudioTreatmentServiceTests.cs` |
| Recovery dispatch | `host/Ams.Cli/Commands/PipelineCommand.cs` (~1419, 1500, 600+) |
| MFA workflow & RunOutcome | `host/Ams.Core/Application/Mfa/MfaWorkflow.cs` |
| Chunk plan | `host/Ams.Core/Services/Alignment/ChunkPlanningService.cs` |
| Chunk corpus | `host/Ams.Core/Application/Mfa/MfaChunkCorpusBuilder.cs` |
| Whisper factory cache | `host/Ams.Core/Processors/AsrProcessor.cs` (FactoryKey at line 19) |
| Timing merger | `host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs` |
| Model resolution | `host/Ams.Core/Asr/AsrEngine.cs` |
| Workstation prep request | `host/Ams.Workstation.Server/Services/Prep/PrepRunSession.cs` |
| Workstation UI | `host/Ams.Workstation.Server/Components/Pages/Prep/Index.razor` |

## Reference: dataset

Failing chapters from `Neighborhood Necromancer 1` (book root: `E:\Audiobooks\Raws\Neighborhood Necromancer 1\` — WSL: `/mnt/e/Audiobooks/Raws/Neighborhood Necromancer 1/`):
- Chapter 3 — Gap 2.82 s (target 1.5 s)
- Chapter 8 — Gap 3.18 s
- Chapter 38 — Gap 3.30 s

After B lands, re-run with `pipeline run --asr-model large-v3 --dtw-timestamps` (the user's standard CLI) and `qc analyze` to verify gap closes to ~1.5 s. If still bad, the issue isn't on the consumer side and A/C become more important.
