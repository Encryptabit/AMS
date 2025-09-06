# AMS Refactor  — Deterministic, Windowed Alignment Pipeline (with Anchors)

Date: 2025-09-06
Audience: AMS contributors (C#/.NET, Python/ML) and integrators
Scope: Planning-only. Canonical contracts, parameters, schemas, CLI, observability, CI, and rollout. FFmpeg remains the renderer; Zig/DSP is not required for this refactor.

This revision elevates n‑gram anchoring and windowing to first‑class, cacheable stages, adds a window‑scoped comparison stage, formalizes an opening‑sentinel policy, specifies a seam zipper in collate, introduces a comparison‑layer normalization contract, clarifies timebase/resampling, and defines an auto‑repair loop. All stages below use a uniform spec (Purpose, Inputs, Parameters, Outputs, Fingerprint, Determinism, CLI, Metrics).

---

## Repository Map (Where Things Live)

- `host/`
  - `Ams.Cli/`: CLI verbs (System.CommandLine): asr, anchors, windows, window-align, refine, collate, script-compare, validate, env, repair
  - `Ams.Core/`: pipeline logic, IO, anchors, windows, transcript index (TX), refinement, validation, comparison layer, repair planning
  - `Ams.Dsp.Native/`: retained for future; not used in this refactor
  - `Ams.UI.Avalonia/`: future desktop UI (out of scope)
- `services/`
  - `aeneas/`: FastAPI wrapper for forced alignment
  - `asr-nemo/`: FastAPI ASR service
- `dsp/`: Zig C ABI (no change; not used by collate in this plan)
- `docs/`: design notes (`ProjectState.md`, `Consolidate.md`)
- `.ams/`: per-input working directory
  - `.ams/timeline/`, `.ams/plan/`, `.ams/chunks/`, `.ams/transcripts/`
  - `.ams/anchors/`, `.ams/windows/`, `.ams/window-align/`
  - `.ams/refine/`, `.ams/collate/`, `.ams/script-compare/`, `.ams/validate/`, `.ams/repair/`

---

## Global Conventions

- Time unit: seconds (double). Sample counts only in chunk-cut metadata.
- Coordinates: chapter space unless explicitly marked as chunk/window‑relative.
- JSON (canonical): UTF‑8 (no BOM); stable key order; doubles ≤6 decimals, no scientific notation; arrays sorted where noted (e.g., toolVersions keys).
- Fingerprint: `sha256(canonical_json(Inputs + Params + ToolVersions))`.
- Paths: absolute. When services run on Linux and host on Windows, convert `C:\…` → `/mnt/c/...` (host only).
- Determinism: prefer CPU for exact reproducibility; when GPU is used, enable deterministic kernels and seeds; document residual drift risk.

Timebase & Resampling (authoritative)
- Authoritative sample rate (SR) for chunking and timestamps: 44100 Hz.
- Allowed resamples: Aeneas may operate at 16000 Hz; ASR may use model‑native SR. Conversion rule is single and explicit: `seconds = samples / authoritative_SR`.
- All stored times are double seconds derived from authoritative SR; rounding occurs only at presentation. Services return seconds; host does not persist service sample counts.

Comparison‑Layer Normalization (comparison only; never persisted)
- Applied by `script-compare` and `validate` only.
- Rules: case folding, punctuation trimming, US↔UK lexicon mapping, proper‑noun/confusion sets.
- Version and hash the rule set; include the hash in fingerprints and in run summary.

Opening‑Sentinel Policy
- Define fixed `[0, 10]` s opening window with relaxed snap rules that must retain ≥ 99.5% of expected words. Validation enforces this hard gate.

---

## Stage Graph (11 Steps; each cacheable)

timeline → plan → chunks → transcripts → anchors → windows → window-align → refine → collate → script-compare → validate

Note: Replaces prior 8‑stage graph. Responsibilities are isolated, fingerprints are stronger, failures are attributable, and reruns can be narrowed to specific stages/windows.

---

## Stage Glossary (11 Steps; Uniform Spec)

Each stage lists: Purpose, Inputs, Parameters, Outputs, Fingerprint, Determinism, CLI, Metrics.

### 1) `timeline` (Detect Silences)
- Purpose: Detect chapter‑wide silences via ffmpeg; compute midpoints.
- Inputs: `input.wav` (abs path); env `FFMPEG_EXE` on PATH.
- Parameters: `dbFloor` (−30.0 dBFS), `minSilenceDur` (0.30 s).
- Outputs:
  - `.ams/timeline/silence..json` { `audioSha256`, `params`, `events`[], `toolVersions`{`ffmpeg`} }
  - `.ams/timeline/status.json`, `.ams/timeline/meta.json`
- Fingerprint: `sha256(input.sha + params + {ffmpeg})`
- Determinism: ffmpeg pinned; deterministic parse; canonical JSON.
- CLI: `ams asr detect-silence --in <audio.wav> --work <dir> --db-floor -30 --min-silence-dur 0.3`
- Metrics: `silenceCount`, `medianDuration`, `p90Duration`, `runtimeMs`.

### 2) `plan` (Window Planning)
- Purpose: Deterministic DP over silence midpoints to produce 60–90 s windows.
- Inputs: `timeline/silence..json`, audio duration.
- Parameters: `min` (60), `max` (90), `target` (75), `strictTail` (true).
- Outputs:
  - `.ams/plan/windows..json` { `windows`[{`start`,`end`}], `params`, `totalCost`, `tailRelaxed` }
  - `.ams/plan/status.json`, `.ams/plan/meta.json`
- Fingerprint: `sha256(silence.sha + params + {plannerVersion})`
- Determinism: stable DP tie‑breaks; canonical JSON.
- CLI: `ams asr plan-windows --in <audio.wav> --work <dir>`
- Metrics: `windowCount`, `p50Sec`, `p90Sec`, `runtimeMs`.

### 3) `chunks` (Audio Cutting)
- Purpose: Cut WAVs per plan.
- Inputs: `plan/windows..json`, `input.wav`.
- Parameters: `format`("wav"), `sampleRate`(44100).
- Outputs:
  - `.ams/chunks/index..json` { `audioSha256`, `params`, `chunks`[{`id`,`span`,`filename`,`sha256`,`durationSec`}]}  
  - `.ams/chunks/wav/<id>.wav`, `.ams/chunks/status.json`, `.ams/chunks/meta.json`
- Fingerprint: `sha256(plan.sha + params + {ffmpeg})`
- Determinism: sample‑exact cuts; canonical JSON.
- CLI: via runner or `ams asr chunks --in <audio.wav> --work <dir>`
- Metrics: `chunkCount`, `totalAudioSec`, `runtimeMs`.

### 4) `transcripts` (ASR Per Chunk)
- Purpose: Transcribe chunks to words with timings.
- Inputs: `.ams/chunks/index..json`, chunk WAVs.
- Parameters: `serviceUrl`(`http://localhost:8081`), `language`(en), `model`(?), `beamSize`(1), `device`(auto|cpu|cuda).
- Outputs:
  - `.ams/transcripts/index..json` { `chunkIds`, `chunkToJsonMap`, `params`, `toolVersions` }
  - `.ams/transcripts/<chunkId>.json` { `chunkId`,`text`,`words`[],`durationSec`,`toolVersions`,`generatedAt` }
  - `.ams/transcripts/status.json`, `.ams/transcripts/meta.json`
- Fingerprint: per chunk `sha256(chunk.sha + params + {serviceVersions})`
- Determinism: CPU exact; GPU seeded; canonical JSON.
- CLI: `ams asr run --from transcripts --to transcripts --in <audio.wav> --work <dir>`
- Metrics: `wordsPerSec`, `rts`, `runtimeMs`.

### 5) `anchors` (Deterministic N‑Gram Selection)
- Purpose: Select LIS‑monotone anchor pairs between BookIndex and ASR tokens; include synthetic start anchor.
- Inputs: BookIndex (canonical), `.ams/transcripts/index..json` (or merged tokens), tokenizer/normalizer config.
- Parameters: `ngram`(3), `relaxDownTo`(2), `targetPerTokens`(0.02), `minSeparation`(50 tokens), `stopwords`(en-basic), `detectSection`(true).
- Outputs:
  - `.ams/anchors/anchors..json` { `meta`{digests,versions}, `params`, `tokens`{counts}, `candidates`[], `selected`[], `stats` }
  - `.ams/anchors/status.json`, `.ams/anchors/meta.json`
- Fingerprint: `sha256(Book.sha + ASR.tokens.sha + params + StopwordsDigest + TokenizerVersion)`
- Determinism: deterministic mining, scoring, LIS selection, tie‑breaks.
- CLI: `ams anchors --book <book.index.json> --asr <chapter.asr.json> --work <dir> --ngram 3 --min-separation 50`
- Metrics: `anchorDensityPer1k`, `relaxationsUsed`, `monotoneViolations`, `runtimeMs`.

### 6) `windows` (Half‑Open Windows with Pads)
- Purpose: Build half‑open windows between anchors; add pads; report coverage/gaps.
- Inputs: `.ams/anchors/anchors..json`, sentences/paragraphs (optional), chapter token counts.
- Parameters: `pre_pad_s`(1.0), `pad_s`(0.6).
- Outputs:
  - `.ams/windows/windows..json` { `meta`{coverage,largestGapSec}, `params`, `windows`[{`id`,`bookStart`,`bookEnd`,`asrStart?`,`asrEnd?`,`prevAnchor`?,`nextAnchor`?}] }
  - `.ams/windows/status.json`, `.ams/windows/meta.json`
- Fingerprint: `sha256(anchors.sha + params)`
- Determinism: stable sentinel/window construction; canonical JSON.
- CLI: `ams windows --work <dir> --pre-pad-s 1.0 --pad-s 0.6`
- Metrics: `coverage`, `largestGapSec`, `windowCount`, `p50Len`, `p90Len`, `runtimeMs`.

### 7) `window-align` (Aeneas per Window with Anchor Guards)
- Purpose: Force alignment within each window; guard with anchors and banded DP.
- Inputs: `.ams/windows/windows..json`, chunk WAVs or chapter WAV, TX‑sliced text from windows, Aeneas service.
- Parameters: `language`(eng), `serviceUrl`(`http://localhost:8082`), `timeoutSec`(600), `bandWidthMs`(600), `hardMonotone`(true), `anchorsImmutable`(true).
- Outputs:
  - `.ams/window-align/<windowId>.aeneas.json` { `windowId`,`offsetSec`,`language`,`textDigest`,`fragments`[],`toolVersions`,`generatedAt` }
  - `.ams/window-align/index..json` (windowId→file), `.ams/window-align/status.json`, `.ams/window-align/meta.json`
- Fingerprint: `sha256(windows.sha + AeneasParams + {python,aeneas})`
- Determinism: fixed text digest; constrained search; canonical JSON.
- CLI: `ams window-align --work <dir> --service http://localhost:8082 --band-width-ms 600`
- Metrics: `windowsAligned`, `meanFragSec`, `alignRuntimeMs`.

### 8) `refine` (Snap to Silence; Anchor‑Aware)
- Purpose: Convert window‑relative begins to chapter time; set `end` to earliest `silence.start` ≥ Aeneas end and < next start. Respect anchor spans; apply opening‑sentinel relaxations.
- Inputs: `.ams/window-align/*.aeneas.json`, `.ams/timeline/silence..json`, `.ams/windows/windows..json`, `.ams/chunks/index..json`.
- Parameters: `silenceThresholdDb`(−38.0), `minSilenceDurSec`(0.12), `min_word_ms`(140), `short_phrase_guard_s`(1.2).
- Outputs:
  - `.ams/refine/sentences..json` { `params`, `sentences`[], `openingSentinel`{`window`:[0,10],`retention`}, `stats` }
  - `.ams/refine/status.json`, `.ams/refine/meta.json`
- Fingerprint: `sha256(window-align.index.sha + silence.sha + params)`
- Determinism: monotone, no overlap, anchor‑aware; canonical JSON.
- CLI: `ams refine --work <dir> --silence-threshold-db -38 --min-silence-dur 0.12`
- Metrics: `adjustedCount`, `avgSnapMs`, `openingRetention`, `runtimeMs`.

### 9) `collate` (Seam Zipper + Roomtone via FFmpeg)
- Purpose: Stitch sentences; replace inter‑sentence gaps and cross‑window slivers with room tone; strict monotone timestamps.
- Inputs: `.ams/refine/sentences..json`, `.ams/windows/windows..json`, original `input.wav`, chunk WAVs.
- Parameters: `roomtoneSource`(auto|file=auto), `roomtoneLevelDb`(−50.0), `minGapMs`(5), `maxGapMs`(2000), `bridgeMaxMs`(60), `zipperHysteresisMs`(50–80), `dedupeWithinOverlap`(true).
- Join algorithm (zipper): dual‑anchor agreement; ±hysteresis; deduplicate tokens within overlap; forbid time backtracks; emit seam counters.
- Outputs:
  - `.ams/collate/segments..json` { `params`, `sentences`, `replacements`[] }
  - `.ams/collate/output.wav` (FFmpeg filter_complex)
  - `.ams/collate/map.json` (join decisions per seam), `.ams/collate/log.json`
  - `.ams/collate/status.json`, `.ams/collate/meta.json`
- Fingerprint: `sha256(refine.sha + params + {ffmpeg})`
- Determinism: bit‑stable given identical inputs/ffmpeg; canonical JSON.
- CLI: `ams collate --work <dir> --roomtone auto --bridge-max-ms 60`
- Metrics: `rtInsertedSec`, `seamDuplications`, `seamOmissions`, `timeBacktracks`, `runtimeMs`.

### 10) `script-compare` (Window‑Scoped, Anchor‑Aware Scoring)
- Purpose: Compare collated transcript vs BookIndex inside anchor windows; produce rich metrics and artifacts.
- Inputs: `book-index.json` (immutable), `.ams/collate/segments..json`, `.ams/anchors/anchors..json`, `.ams/windows/windows..json`; comparison layer rules/lexicon.
- Parameters: comparison rules version/hash; costs for WER/CER.
- Outputs:
  - `.ams/script-compare/report.json` (chapter + per‑window + per‑sentence metrics)
  - `.ams/script-compare/map.jsonl` (one line per book word; =|S|I|D with times)
  - `.ams/script-compare/errors.csv`, `.ams/script-compare/diff.txt`
  - `.ams/script-compare/status.json`, `.ams/script-compare/meta.json`
- Required metrics:
  - `wer`, `cer` (chapter + per window)
  - `opening_retention_0_10s`
  - `short_phrase_loss_rate` (≤ 1.2 s phrases)
  - `seam_duplications`, `seam_omissions` (counts & examples)
  - `anchor_coverage`, `anchor_drift_p50`, `anchor_drift_p95`
- Fingerprint: `sha256(segments.sha + anchors.sha + windows.sha + comparisonRules.hash + params)`
- Determinism: comparison‑only normalization; canonical outputs.
- CLI: `ams script-compare --book <book.index.json> --work <dir>`
- Metrics: all above plus `runtimeMs`.

### 11) `validate` (Gates & Auto‑Repair Plan)
- Purpose: Enforce gates over `script-compare` metrics and generate an optional repair plan for failing windows.
- Inputs: `.ams/script-compare/report.json`, `.ams/windows/windows..json`.
- Parameters (gates):
  - `opening_retention_0_10s ≥ 0.995`
  - `seam_duplications = 0`, `seam_omissions = 0`
  - `short_phrase_loss_rate ≤ 0.005`
  - `anchor_drift_p95 ≤ 0.8 s`, `anchor_coverage ≥ 0.85`
  - `wer ≤ project_threshold`, `cer ≤ project_threshold`
- Outputs:
  - `.ams/validate/report..json` { `gates`, `metrics`, `status`("pass|fail") }
  - On failure: `.ams/repair/repair.plan.json` with window IDs and suggestions (e.g., `pad_s +0.2`, `addSoftAnchor:true`), plus `.ams/validate/status.json`, `.ams/validate/meta.json`.
- Fingerprint: `sha256(script-compare.report.sha + gates)`
- Determinism: deterministic evaluation; canonical JSON.
- CLI: `ams validate --work <dir> [--emit-repair]`
- Metrics: `failedGates`, `topFailingWindows[]`, `runtimeMs`.

Repair verb (optional but recommended)
- CLI: `ams repair --work <dir> --plan .ams/repair/repair.plan.json`
- Behavior: re‑run only listed windows through `windows → window-align → refine → collate → script-compare` and re‑validate.

---

## Anchor & Window Invariants
- Anchors are non‑overlapping; LIS‑monotone in (bookPos, asrPos); synthetic start anchor mandatory.
- Windows are half‑open, padded (`pre_pad_s`, `pad_s`), and cover the chapter except intentional long gaps; report `coverage` and `largestGapSec`.
- Downstream stages operate inside windows with documented pads. No stage may intrude into anchor spans.

---

## Manifest  (Top‑Level Contract)
- Path: `.ams/manifest.json`
- Fields: `schemaVersion`("2.0"), `input`{`path`,`sha256`,`durationSec`,`sizeBytes`,`mtimeUtc`}, `stages`(name→StageEntry), `created`, `modified`.
- `StageEntry`: `status`{`status`,`started`,`ended`,`attempts`,`error?`}, `artifacts`(name→path), `metrics?`(name→path), `fingerprint`{`inputHash`,`paramsHash`,`toolVersions`{k→v}}.
- Serialization: canonical JSON (see Global Conventions).

---

## Service APIs (Versioned; Single Path‑Mapping Note)

ASR (asr-nemo)
- Base: `http://localhost:8081`
- `GET /v1/health` → `{status:"ok"}`
- `GET /v1/version` → `{service:"asr-nemo",version:"x.y.z",torch:"2.3.1",cuda?:"12.2"}`
- `POST /v1/transcribe` { `audio_path`,`language`,`model?`,`beamSize?`,`device?` } → `{ text, words[], durationSec, toolVersions, generatedAt }`

Alignment (aeneas)
- Base: `http://localhost:8082`
- `GET /v1/health`, `GET /v1/version`
- `POST /v1/align-chunk` { `chunk_id`,`audio_path`,`lines`[],`language`,`timeout_sec` } → `{ windowId?, offsetSec, fragments[], toolVersions, generatedAt }`

Path mapping policy (host): convert Windows paths to WSL‑style for Linux services; services never guess mapping.

---

## Determinism & Fingerprints (Exact Rules)
- Inputs referenced by SHA256 (files) or canonical JSON SHA (artifacts).
- Params serialized canonically with explicit defaults included.
- ToolVersions map sorted by key when hashing.
- Hash: `sha256(JSON(Inputs) + "\n" + JSON(Params) + "\n" + JSON(ToolVersions))`.
- GPU determinism: enable deterministic kernels; fixed seeds; prefer CPU for exactness.

---

## Observability Upgrades
- Per‑window scorecards (JSONL) in `script-compare/map.jsonl` with anchors hit, drift, WER, retention, seam counters.
- Run summary includes top 5 failing windows with artifact links.
- Seam log (collate): overlap span, kept side, dedup count, any backtracks.

---

## Testing & CI Gates
- Unit: tokenizer normalization; stopword guards; LIS selection; windows coverage; TX invariants; fingerprint stability; zipper monotonicity.
- Integration (mocked): anchors/windows/window-align on tiny assets; byte‑exact JSON; seam map sanity.
- E2E (tiny assets): services via compose; run through `script-compare`; compare JSON digests; WAV duration check.
- Metrics gate on tiny assets: `opening_retention=1.0`, `seam dup/omission=0`, `drift p95 ≤ 0.6 s`, `WER/CER within bounds`.
- PRs touching alignment/refine/collate must pass the metrics gate.

---

## Parameter Defaults (Doc‑Only)
- Anchors: `ngrams=[3,4,5]`, `minSeparation=50 tokens`, `targetPerTokens≈0.02`, `fuzzy≥0.85` (if used), `pre_pad_s=1.0`, `pad_s=0.6`.
- Window‑align: `bandWidthMs≈600`, `hardMonotone=true`, `anchorsImmutable=true`.
- Refine: `min_word_ms=140`, `short_phrase_guard_s=1.2`, `silenceThresholdDb=−38 dBFS`.
- Collate zipper: `hysteresis 50–80 ms`, `dedupeWithinOverlap=true`, `monotone enforcement=true`.
- Validate gates: see Stage 11.

---

## Risks & Rollback
- Sparse anchors: relax `n`, increase pads, or add soft anchors (via repair plan).
- Noisy starts: increase `pre_pad_s` in opening sentinel.
- Service drift: pin `/v1/version` in fingerprints; reject unknown versions.

---

## Appendix — Pseudocode (Anchors/Windows/TX)
```
anchors = mine_ngrams(bookTokens, asrTokens, n, stopwords)
anchors = relax_if_sparse(anchors, n, relaxDownTo, minSeparation)
anchors = score_and_sort_deterministically(anchors)
selected = LIS_monotone(anchors by (bp, ap), tie_break=(bp,ap))
windows  = build_half_open_windows(selected, pads=(pre_pad_s,pad_s), sentinels=true)
tx       = intersect_windows_with_sentences(windows, sentences)
```

---

## Example CLI Flow (Tiny Asset)
```
ams asr detect-silence --in chapter.wav --work chapter.wav.ams
ams asr plan-windows   --in chapter.wav --work chapter.wav.ams
ams asr run            --from chunks --to transcripts --in chapter.wav --work chapter.wav.ams --parallel 2
ams anchors            --book book.index.json --asr chapter.asr.json --work chapter.wav.ams --ngram 3 --min-separation 50
ams windows            --work chapter.wav.ams --pre-pad-s 1.0 --pad-s 0.6
ams window-align       --work chapter.wav.ams --service http://localhost:8082 --band-width-ms 600
ams refine             --work chapter.wav.ams --silence-threshold-db -38 --min-silence-dur 0.12
ams collate            --work chapter.wav.ams --roomtone auto --bridge-max-ms 60
ams script-compare     --book book.index.json --work chapter.wav.ams
ams validate           --work chapter.wav.ams --emit-repair
# Optional fast iteration
ams repair             --work chapter.wav.ams --plan .ams/repair/repair.plan.json
```

End of Refactor  (windowed alignment, anchors first‑class, ffmpeg renderer).

