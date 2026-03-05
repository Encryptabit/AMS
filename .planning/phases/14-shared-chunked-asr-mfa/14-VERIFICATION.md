# Phase 14: Shared Chunked ASR/MFA - Verification & Benchmark Checklist

## Purpose

Go/no-go criteria for promoting shared chunking as the default pipeline behavior.
All entries should be completed with actual runtime measurements before rollout.

## Prerequisites

- [ ] Nemo ASR service running at configured endpoint
- [ ] MFA conda environment activated and models downloaded (english_us_arpa dictionary/acoustic/g2p)
- [ ] Test book with multiple chapters available (minimum 3 chapters recommended)
- [ ] Working directory configured with sufficient disk space for parallel MFA workspaces

---

## 1. Run Matrix

### 1.1 Single Chapter - Baseline vs Chunked

| Run | Config | ASR Time | MFA Time | Total Time | Status |
|-----|--------|----------|----------|------------|--------|
| A1  | `--no-chunk-plan --no-chunked-mfa` (legacy) | ___ | ___ | ___ | |
| A2  | Default (shared chunking enabled) | ___ | ___ | ___ | |
| A3  | `--no-chunked-mfa` (ASR chunked, MFA legacy) | ___ | ___ | ___ | |
| A4  | `--no-chunk-plan` (ASR legacy, MFA auto) | ___ | ___ | ___ | |

**Commands:**
```powershell
# A1: Full legacy baseline
dotnet Ams.Cli.dll pipeline run --book <book> --audio <chapter.wav> --work-dir <dir> --force --no-chunk-plan --no-chunked-mfa

# A2: Default chunked (no rollout flags)
dotnet Ams.Cli.dll pipeline run --book <book> --audio <chapter.wav> --work-dir <dir> --force

# A3: ASR chunked, MFA legacy
dotnet Ams.Cli.dll pipeline run --book <book> --audio <chapter.wav> --work-dir <dir> --force --no-chunked-mfa

# A4: ASR legacy, MFA uses whatever plan exists
dotnet Ams.Cli.dll pipeline run --book <book> --audio <chapter.wav> --work-dir <dir> --force --no-chunk-plan
```

**Acceptance:** A2 total time <= A1 total time. Quality metrics (section 2) within tolerance.

### 1.2 All Chapters - Parallelism Scaling

| Run | max-mfa | Chapters | Wall Clock | Avg/Chapter | Peak Memory | Status |
|-----|---------|----------|------------|-------------|-------------|--------|
| B1  | 1       | all      | ___        | ___         | ___         | |
| B2  | 2       | all      | ___        | ___         | ___         | |
| B3  | 4       | all      | ___        | ___         | ___         | |
| B4  | CPU/2   | all      | ___        | ___         | ___         | |

**Commands:**
```powershell
# B1-B4: Vary --max-mfa with all chapters via REPL mode
# In REPL: `mode all` then `pipeline run --force --max-mfa <N>`
```

**Acceptance:** B2 wall clock < B1 wall clock. Diminishing returns expected beyond B3.

### 1.3 MFA Beam Profile Comparison

| Run | Profile    | Beam/Retry | MFA Time | Coverage | Retry Count | Status |
|-----|------------|------------|----------|----------|-------------|--------|
| C1  | fast       | 20/80      | ___      | ___      | ___         | |
| C2  | balanced   | 40/120     | ___      | ___      | ___         | |
| C3  | strict     | 80/200     | ___      | ___      | ___         | |

**Commands:**
```powershell
# C1-C3: Single chapter with each profile
dotnet Ams.Cli.dll pipeline run --book <book> --audio <chapter.wav> --work-dir <dir> --force --mfa-profile fast
dotnet Ams.Cli.dll pipeline run --book <book> --audio <chapter.wav> --work-dir <dir> --force --mfa-profile balanced
dotnet Ams.Cli.dll pipeline run --book <book> --audio <chapter.wav> --work-dir <dir> --force --mfa-profile strict
```

**Acceptance:** `balanced` is reasonable default (acceptable coverage without strict's time penalty).

---

## 2. Quality Parity Checks

### 2.1 Merge Coverage

Compare timing merge results between legacy and chunked paths.

| Metric | Legacy (A1) | Chunked (A2) | Delta | Acceptable |
|--------|-------------|--------------|-------|------------|
| Sentences with MFA timings | ___ | ___ | ___ | <= 1% regression |
| Words with MFA timings | ___ | ___ | ___ | <= 1% regression |
| Sentence update count (merge) | ___ | ___ | ___ | Within 5% |
| Average timing delta per word (ms) | ___ | ___ | ___ | < 50ms |

**How to measure:**
- Run `pipeline run` with `--verbose` and capture merge log output
- Compare `MergeTimingsCommand` log lines: "Updated N of M sentences"
- Diff the resulting `.align.hydrate.json` files between A1 and A2

### 2.2 TextGrid Completeness

| Check | Legacy | Chunked | Pass |
|-------|--------|---------|------|
| TextGrid file exists | ___ | ___ | |
| Words tier interval count | ___ | ___ | |
| Phones tier interval count | ___ | ___ | |
| Monotonically increasing timestamps | ___ | ___ | |
| No overlapping intervals | ___ | ___ | |
| Gap coverage (% of audio duration) | ___ | ___ | |

**How to measure:**
- Parse TextGrid files and count intervals per tier
- Verify timestamp ordering: each interval's start >= previous interval's end
- Calculate coverage: sum of non-silence interval durations / total audio duration

---

## 3. Failure & Retry Metrics

### 3.1 MFA Alignment Failures

| Scenario | Count | Behavior | Expected |
|----------|-------|----------|----------|
| Chunk alignment failures (balanced) | ___ | Adaptive retry with strict | Low-quality chunks retried |
| Chunk alignment failures (fast) | ___ | Adaptive retry with strict | More retries expected |
| Total retry invocations | ___ | ___ | < 20% of chunks |
| Retry success rate | ___ | ___ | > 80% |

### 3.2 Edge Cases

| Test | Description | Result | Pass |
|------|-------------|--------|------|
| Empty chapter | Audio with no speech content | ___ | Graceful skip |
| Very short chapter | < 10 seconds of audio | ___ | Single chunk, legacy path |
| Very long chapter | > 60 minutes of audio | ___ | Multiple chunks, no timeout |
| Single-chunk plan | Audio with no silence boundaries | ___ | Falls to legacy path |

---

## 4. MFA Workspace Isolation

### 4.1 Parallel Workspace Behavior

| Check | Expected | Actual | Pass |
|-------|----------|--------|------|
| Workspace directories created | MFA_1 through MFA_N (N = max-mfa) | ___ | |
| No cross-workspace file contamination | Each workspace has only its own artifacts | ___ | |
| Workspace leasing (rent/return) | Queue-based, no starvation | ___ | |
| Command history isolation | Each workspace has independent history | ___ | |
| Cleanup after pipeline complete | No orphaned lock files or temp artifacts | ___ | |

### 4.2 Workspace Recovery

| Check | Expected | Actual | Pass |
|-------|----------|--------|------|
| Interrupted pipeline resume | Next run starts clean | ___ | |
| Corrupted command_history.yaml | ResetCommandHistoryFile recovers | ___ | |
| Missing workspace directory | EnsureWorkspace creates it | ___ | |
| Concurrent chapter access to same workspace | Semaphore prevents overlap | ___ | |

**How to verify:**
```powershell
# Run multi-chapter pipeline, then inspect workspace directories:
Get-ChildItem "$env:USERPROFILE\Documents\MFA_*" -Directory | ForEach-Object {
    Write-Host $_.Name
    Get-ChildItem $_.FullName -File | Select-Object Name, Length
}

# Verify no orphan files after clean run:
# Each MFA_N should contain only: command_history.yaml (if any)
# Alignment output dirs should be under chapter work dirs, not workspace roots
```

---

## 5. Rollout Flag Verification

### 5.1 Flag Behavior

| Flag | Stage Affected | Expected Behavior | Verified |
|------|---------------|-------------------|----------|
| `--no-chunk-plan` | ASR | Single-buffer ASR, no chunk plan generated | |
| `--no-chunked-mfa` | MFA | Single-utterance corpus, chunk plan ignored by MFA | |
| Both flags | ASR + MFA | Full legacy behavior, identical to pre-Phase 14 | |
| Neither flag (default) | ASR + MFA | Shared chunk plan, chunked MFA corpus | |
| `--no-chunk-plan` only | ASR legacy, MFA auto | MFA falls to legacy (no plan available) | |
| `--no-chunked-mfa` only | ASR chunked, MFA legacy | ASR generates plan, MFA ignores it | |

### 5.2 Backward Compatibility

| Check | Expected | Verified |
|-------|----------|----------|
| Existing pipeline commands work without new flags | Yes (defaults preserve new behavior) | |
| REPL `mode all` + `pipeline run` works with flags | Yes | |
| Single chapter mode works with flags | Yes | |
| Workstation server pipeline (no CLI flags) | Uses defaults, chunking enabled | |

---

## 6. Concurrency Default Guidance

### Recommended Settings

| Setting | Default Value | Rationale |
|---------|--------------|-----------|
| `--max-asr` | 1 | GPU-bound; single large model saturates VRAM |
| `--max-mfa` | CPU_COUNT / 2 | CPU-bound; each MFA process is multi-threaded |
| `--max-workers` | CPU_COUNT | Overall chapter parallelism limit |
| `--mfa-profile` | balanced | Reasonable coverage/speed tradeoff |

### Environment-Specific Tuning

| Environment | max-asr | max-mfa | max-workers | Notes |
|-------------|---------|---------|-------------|-------|
| Single GPU, 8-core | 1 | 4 | 8 | Standard workstation |
| Single GPU, 16-core | 1 | 8 | 16 | High-core server |
| Multi-GPU, 16-core | 2 | 8 | 16 | Rare; requires ASR service sharding |
| CPU-only, 8-core | 1 | 4 | 4 | Slower ASR, MFA can run alongside |

---

## 7. Sign-Off Checklist

| # | Criterion | Status | Signed Off By |
|---|-----------|--------|---------------|
| 1 | Single-chapter: chunked total time <= legacy total time | | |
| 2 | Multi-chapter: max-mfa=2 provides wall-clock improvement | | |
| 3 | Quality: merge coverage regression < 1% | | |
| 4 | Quality: TextGrid timestamps monotonically valid | | |
| 5 | Workspace: no cross-contamination under parallel load | | |
| 6 | Workspace: cleanup leaves no orphaned artifacts | | |
| 7 | Rollout flags: all 6 combinations behave correctly | | |
| 8 | Backward compat: existing commands work without new flags | | |
| 9 | Retry: adaptive strict retry succeeds on >80% of retried chunks | | |
| 10 | Edge cases: empty/short/long chapters handled gracefully | | |

**Go decision:** All 10 criteria must be met. Criteria 1-4 require measured data. Criteria 5-10 can be verified by inspection or manual testing.

---

*Phase: 14-shared-chunked-asr-mfa*
*Document created: 2026-03-05*
*Status: Template ready -- populate with runtime measurements before rollout decision*
