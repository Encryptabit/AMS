# Pruning Plan

**Phased dead code removal plan with exact file paths and commands**

Generated: 2025-12-30
Source: Phase 3 artifacts (DEAD-CODE.md, ORPHANED-FILES.md, UNUSED-METHODS.md)

---

## Executive Summary

| Metric | Value |
|--------|-------|
| Total files to remove | 10 |
| Total methods to remove | 9 |
| Total lines removable | ~650 |
| Risk level | LOW |
| Estimated effort | 2-4 hours |

All items identified through grep verification with zero incoming dependencies.

---

## Phase 1: Quick Wins (< 1 hour, ~90 lines)

### 1.1 Empty Placeholders (4 files, 32 lines)

Whisper.NET scaffolding that was never implemented. Real Whisper.NET integration lives in `AsrProcessor.cs`.

| File | Lines | Issue ID |
|------|-------|----------|
| `host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnModel.cs` | 8 | AUD-008 |
| `host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnSession.cs` | 8 | AUD-008 |
| `host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnTranscriber.cs` | 8 | AUD-008 |
| `host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnUtils.cs` | 8 | AUD-008 |

**Removal commands:**
```bash
git rm host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnModel.cs
git rm host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnSession.cs
git rm host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnTranscriber.cs
git rm host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnUtils.cs
```

**Verification:**
```bash
grep -r "WnModel\|WnSession\|WnTranscriber\|WnUtils" --include="*.cs" host/
# Expected: No matches (only self-references)
```

### 1.2 Template Artifacts (1 file, 5 lines)

Empty class from project template generation.

| File | Lines | Issue ID |
|------|-------|----------|
| `host/Ams.Web.Shared/Class1.cs` | 5 | AUD-010 |

**Removal command:**
```bash
git rm host/Ams.Web.Shared/Class1.cs
```

**Verification:**
```bash
grep -r "Class1" --include="*.cs" host/
# Expected: No matches
```

### 1.3 Superseded Manifest (1 file, 19 lines)

Unused manifest format, superseded by ChapterContext and document slot pattern.

| File | Lines | Issue ID |
|------|-------|----------|
| `host/Ams.Core/Pipeline/ManifestV2.cs` | 19 | AUD-009 |

**Removal command:**
```bash
git rm host/Ams.Core/Pipeline/ManifestV2.cs
```

**Verification:**
```bash
grep -r "ManifestV2" --include="*.cs" host/
# Expected: No matches
```

### 1.4 Empty Service Interface (2 files, 18 lines)

Interface and empty implementation with zero consumers.

| File | Lines | Issue ID |
|------|-------|----------|
| `host/Ams.Core/Services/Interfaces/IAudioService.cs` | 5 | AUD-005 |
| `host/Ams.Core/Services/AudioService.cs` | 13 | AUD-005 |

**Removal commands:**
```bash
git rm host/Ams.Core/Services/Interfaces/IAudioService.cs
git rm host/Ams.Core/Services/AudioService.cs
```

**Verification:**
```bash
grep -r "IAudioService\|AudioService" --include="*.cs" host/
# Expected: No matches for IAudioService, only AudioProcessor matches for AudioService
```

### Phase 1 Summary

| Item | Files | Lines |
|------|-------|-------|
| Empty placeholders | 4 | 32 |
| Template artifact | 1 | 5 |
| Superseded manifest | 1 | 19 |
| Empty service | 2 | 18 |
| **Total** | **8** | **~74** |

**Estimated time:** 15-30 minutes
**Risk:** NONE (all verified as truly orphaned)

---

## Phase 2: Verified Removals (1-2 hours, ~470 lines)

### 2.1 Demo Code (1 file, 141 lines)

Demo runner for DSP operations with no production callers.

| File | Lines | Issue ID |
|------|-------|----------|
| `host/Ams.Core/Audio/DspDemoRunner.cs` | 141 | AUD-001 |

**Verification before removal:**
```bash
grep -r "DspDemoRunner\|RunDemo" --include="*.cs" host/
# Expected: Only self-reference in DspDemoRunner.cs
```

**Removal command:**
```bash
git rm host/Ams.Core/Audio/DspDemoRunner.cs
```

**Alternative:** If DSP regression testing is valuable, move to `test/Ams.Tests/DspDemoTests.cs` instead of deleting.

### 2.2 Unused Timeline Builder (1 file, 182 lines)

Zero callers for `Build()` method, likely superseded by PauseMapBuilder.

| File | Lines | Issue ID |
|------|-------|----------|
| `host/Ams.Core/Audio/SentenceTimelineBuilder.cs` | 182 | AUD-002 |

**Verification before removal:**
```bash
grep -r "SentenceTimelineBuilder\|SentenceTimelineEntry" --include="*.cs" host/
# Expected: Only self-references
```

**Removal command:**
```bash
git rm host/Ams.Core/Audio/SentenceTimelineBuilder.cs
```

### 2.3 Unused AudioProcessor Methods (~145 lines)

Methods in `AudioProcessor.cs` with zero external callers.

| Method | Lines (est.) | Issue ID |
|--------|--------------|----------|
| `AdjustVolume` | 15 | AUD-011 |
| `FadeOut` | 15 | AUD-011 |
| `EncodeWav` | 10 | AUD-011 |
| `NormalizeLoudness` | 40 | AUD-011 |

**Verification:**
```bash
grep -r "\.AdjustVolume\|\.FadeOut\|\.EncodeWav\|\.NormalizeLoudness" --include="*.cs" host/
# Expected: No matches (only definitions)
```

**File to edit:** `host/Ams.Core/Audio/AudioProcessor.cs`

Remove the following methods:
- `public static AudioBuffer AdjustVolume(AudioBuffer buffer, double gain)` (~15 lines)
- `public static AudioBuffer FadeOut(AudioBuffer buffer, double durationMs)` (~15 lines)
- `public static void EncodeWav(AudioBuffer buffer, string outputPath)` (~10 lines)
- `public static AudioBuffer NormalizeLoudness(AudioBuffer buffer, double targetLufs)` (~40 lines)

### 2.4 Unused AudioProcessor.Analysis Methods (~65 lines)

Methods in `AudioProcessor.Analysis` nested class with zero callers.

| Method | Lines (est.) | Issue ID |
|--------|--------------|----------|
| `AnalyzeGap` | 30 | AUD-011 |
| `FindSpeechEndFromGap` | 25 | AUD-011 |
| `SnapToEnergyAuto` | 25 | AUD-011 |

**Verification:**
```bash
grep -r "\.AnalyzeGap\|\.FindSpeechEndFromGap\|\.SnapToEnergyAuto" --include="*.cs" host/
# Expected: No matches (only definitions and commented-out reference)
```

**File to edit:** `host/Ams.Core/Audio/AudioProcessor.cs` (Analysis nested class)

### 2.5 Duplicate Methods in AlignmentService (~25 lines)

Duplicated in ChapterContext. Remove from AlignmentService.

| Method | Lines (est.) | Issue ID |
|--------|--------------|----------|
| `TryExtractChapterNumber` | 10 | AUD-012 |
| `EnumerateLabelCandidates` | 15 | AUD-012 |

**File to edit:** `host/Ams.Core/Services/Alignment/AlignmentService.cs`

**Note:** These duplicates should be removed AFTER ChapterLabelResolver is extracted (see CONSOLIDATION-PLAN.md AUD-004).

### Phase 2 Summary

| Item | Files | Lines |
|------|-------|-------|
| DspDemoRunner.cs | 1 | 141 |
| SentenceTimelineBuilder.cs | 1 | 182 |
| AudioProcessor methods | (edit) | ~145 |
| AlignmentService duplicates | (edit) | ~25 |
| **Total** | **2 files + 2 edits** | **~493** |

**Estimated time:** 1-2 hours
**Risk:** LOW (all verified with grep)

---

## Phase 3: Review Required (Discussion Needed)

### 3.1 Analysis Tools (MEDIUM confidence)

These are standalone utilities that may still be useful.

| File | Lines | Issue ID | Status |
|------|-------|----------|--------|
| `analysis/OverlayTest/Program.cs` | 24 | AUD-006 | BUILD FAILS - Remove |
| `out/InspectDocX/Program.cs` | 14 | AUD-023 | Works - Archive |

**OverlayTest Decision:** BUILD BROKEN - references removed `AudioProcessor.OverlayRoomtone()` method. **Recommend: DELETE** immediately.

```bash
# Remove entire project
rm -rf analysis/OverlayTest/
```

**InspectDocX Decision:** Still builds but is one-time utility. **Recommend: ARCHIVE** to `archive/` or delete.

### 3.2 Dormant UI Project (MEDIUM confidence)

| Project | Files | Lines | Issue ID |
|---------|-------|-------|----------|
| `host/Ams.UI.Avalonia/` | 3 | 45 | AUD-019, AUD-024 |

**Files:**
- `App.axaml.cs` (22 lines)
- `MainWindow.axaml.cs` (10 lines)
- `Program.cs` (13 lines)

**Decision required:** Archive, remove from solution, or keep for future desktop UI.

**Recommendation:** Archive to `archive/Ams.UI.Avalonia/` - preserves code for potential future use without cluttering active solution.

### Phase 3 Summary

| Item | Files | Lines | Decision |
|------|-------|-------|----------|
| OverlayTest | 1+ | 24 | DELETE (broken) |
| InspectDocX | 1+ | 14 | ARCHIVE |
| Ams.UI.Avalonia | 3+ | 45 | ARCHIVE |
| **Total** | **5+** | **~83** | **Discussion** |

---

## Summary Table

| Phase | Items | Lines | Time | Risk |
|-------|-------|-------|------|------|
| Phase 1: Quick Wins | 8 files | ~74 | 15-30 min | NONE |
| Phase 2: Verified | 2 files + 2 edits | ~493 | 1-2 hours | LOW |
| Phase 3: Review | 5+ files | ~83 | Discussion | LOW |
| **Total** | **15+ items** | **~650** | **2-4 hours** | **LOW** |

---

## Pre-Removal Checklist

Before executing any removals:

- [ ] Create feature branch: `git checkout -b prune/dead-code-removal`
- [ ] Verify clean build before changes: `dotnet build`
- [ ] Run tests before changes: `dotnet test`
- [ ] Back up any files with uncertain status

### Phase 1 Execution Checklist

- [ ] Verify Wn*.cs have no callers (grep)
- [ ] Remove 4 Whisper.NET placeholders
- [ ] Verify Class1.cs has no callers
- [ ] Remove Class1.cs
- [ ] Verify ManifestV2 has no callers
- [ ] Remove ManifestV2.cs
- [ ] Verify IAudioService has no consumers
- [ ] Remove IAudioService.cs and AudioService.cs
- [ ] Build and test
- [ ] Commit Phase 1 changes

### Phase 2 Execution Checklist

- [ ] Verify DspDemoRunner.cs has no production callers
- [ ] Remove or relocate DspDemoRunner.cs
- [ ] Verify SentenceTimelineBuilder.cs has no callers
- [ ] Remove SentenceTimelineBuilder.cs
- [ ] Remove unused AudioProcessor methods (one at a time, test after each)
- [ ] Remove duplicate AlignmentService methods (after consolidation)
- [ ] Build and test
- [ ] Commit Phase 2 changes

### Phase 3 Execution Checklist

- [ ] Get team approval for OverlayTest removal
- [ ] Delete OverlayTest project
- [ ] Decide on InspectDocX (archive or delete)
- [ ] Decide on Ams.UI.Avalonia (archive, remove, or keep)
- [ ] Execute decided actions
- [ ] Build and test
- [ ] Commit Phase 3 changes

---

## Verification Commands Reference

```bash
# General orphan check for a class name
grep -r "ClassName" --include="*.cs" .

# Check for method invocations
grep -r "\.MethodName(" --include="*.cs" .

# Check for type usage
grep -r ": TypeName\|new TypeName\|TypeName\." --include="*.cs" .

# Build verification
dotnet build host/Ams.Core/Ams.Core.csproj
dotnet build

# Test verification
dotnet test test/Ams.Tests/Ams.Tests.csproj
```

---

## Impact Analysis

### Dependencies That Would Break

**None.** All identified dead code has zero incoming dependencies. Verified through:
1. Grep search for callers across all .cs files
2. Interface implementation checks
3. Build dependency analysis

### Test Coverage Affected

**Minimal.** No tests directly test the removed code:
- DspDemoRunner.cs - Not tested (could become a test itself)
- SentenceTimelineBuilder.cs - Not tested
- Unused methods - Not tested

### Build Impact

**None.** All projects will continue to build after removals.

---

*Generated: 2025-12-30*
*Source: Phase 3 Code Audit (DEAD-CODE.md, ORPHANED-FILES.md, UNUSED-METHODS.md)*
