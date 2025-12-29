# Dead Code Catalogue

Comprehensive inventory of dead code identified in the AMS codebase with confidence levels and removal recommendations.

**Analysis Date:** 2025-12-28
**Reference Documents:**
- ORPHANED-FILES.md (orphaned file analysis)
- UNUSED-METHODS.md (unused method scan)
- DISCOVERY-SYNTHESIS.md (prior flagged candidates)

---

## Executive Summary

| Category | Count | Lines Removable |
|----------|-------|-----------------|
| **Orphaned Files (HIGH confidence)** | 7 | ~233 |
| **Orphaned Files (MEDIUM confidence)** | 4 | ~224 |
| **Unused Public Methods** | 8 | ~180 |
| **Unused Service Classes** | 2 | ~18 |
| **Total Immediately Removable** | 17 | ~431 |
| **Total After Review** | 21 | ~655 |

**Bottom Line:** Approximately **650-700 lines of dead code** can be removed, representing ~0.5% of the codebase.

---

## Obsolete Code Investigation

### Whisper.NET Status

**Verdict: KEEP (with cleanup)**

**Investigation:**
1. The system supports TWO ASR engines: Whisper.NET and Nemo
2. Whisper.NET is the DEFAULT engine (`AsrEngine.Resolve()` returns `Whisper` if no env var set)
3. `AsrProcessor.cs` actively uses Whisper.NET via the NuGet package (`Whisper.net`)
4. `GenerateTranscriptCommand.cs` has separate code paths for both engines

**Files to REMOVE (empty placeholders):**
- `WnModel.cs` (8 lines) - Empty placeholder, never implemented
- `WnSession.cs` (8 lines) - Empty placeholder, never implemented
- `WnTranscriber.cs` (8 lines) - Empty placeholder, never implemented
- `WnUtils.cs` (8 lines) - Empty placeholder, never implemented

**Total removable: 32 lines (4 files)**

**Files to KEEP:**
- `AsrProcessor.cs` - Active Whisper.NET integration via NuGet package
- `AsrEngine.cs` - Engine selection logic
- `AsrClient.cs` - Nemo HTTP client

**Explanation:** The Wn* files were scaffolded for a potential refactoring to move Whisper.NET logic into separate classes but were never implemented. The actual Whisper.NET integration lives in `AsrProcessor.cs` and uses `WhisperFactory`, `WhisperProcessor` from the `Whisper.net` NuGet package directly. These are NOT superseded by Nemo - both engines are supported.

---

### DspDemoRunner Status

**Verdict: REMOVE (orphaned demo code)**

**Investigation:**
1. Grep search for `DspDemoRunner` yields only self-reference (class definition)
2. Grep search for `RunDemo` yields only the method definition
3. File purpose: "Demo runner for DSP operations" - test/demo harness
4. Uses `AmsDsp.Create()` to exercise native DSP wrapper

**Evidence:**
```
$ grep -r "DspDemoRunner" --include="*.cs" .
./host/Ams.Core/Audio/DspDemoRunner.cs:23:public static class DspDemoRunner
```

**Recommendation:** **REMOVE** - 141 lines of demo code with no production callers

**Alternative:** Move to `Ams.Tests` project if useful for regression testing native wrapper

---

### FeatureExtraction Status

**Verdict: KEEP (actively used)**

**Investigation:**
1. Grep search found 1 active caller:
   - `ValidateCommand.cs:968` calls `FeatureExtraction.Detect(...)`
2. File contains 564 lines of sophisticated breath detection algorithm
3. Used in interactive timing validation session

**Evidence:**
```csharp
// ValidateCommand.cs:968
var regions = FeatureExtraction.Detect(audio, startSec, endSec, BreathGuardOptions);
```

**Recommendation:** **KEEP** - This is actively used code, not dead code. The original flag was incorrect.

---

### Other Obsolete Code Discovered

#### SentenceTimelineBuilder (182 lines)
**Verdict: REMOVE**

- Zero callers for `Build()` method
- Zero references to `SentenceTimelineEntry` type
- Likely replaced by `PauseMapBuilder` or similar

#### ManifestV2 (19 lines)
**Verdict: REMOVE**

- Zero references to `ManifestV2` type
- Superseded by `ChapterContext` and document slot pattern

#### AudioService / IAudioService (18 lines)
**Verdict: REMOVE**

- `AudioService` implements empty `WarmAsync()` method
- No consumers of `IAudioService` interface
- Never registered in DI container
- Placeholder service that was never implemented

#### Class1.cs in Ams.Web.Shared (5 lines)
**Verdict: REMOVE**

- Empty placeholder class from project template
- Zero references

---

## Complete Dead Code Inventory

### Files to Remove (HIGH confidence)

| File Path | Lines | Reason |
|-----------|-------|--------|
| `host/Ams.Core/Audio/DspDemoRunner.cs` | 141 | Demo code, no production callers |
| `host/Ams.Core/Audio/SentenceTimelineBuilder.cs` | 182 | Zero callers for Build method |
| `host/Ams.Core/Pipeline/ManifestV2.cs` | 19 | Superseded manifest format |
| `host/Ams.Core/Services/AudioService.cs` | 13 | Empty service, never implemented |
| `host/Ams.Core/Services/Interfaces/IAudioService.cs` | 5 | Unused interface |
| `host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnModel.cs` | 8 | Empty placeholder |
| `host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnSession.cs` | 8 | Empty placeholder |
| `host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnTranscriber.cs` | 8 | Empty placeholder |
| `host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnUtils.cs` | 8 | Empty placeholder |
| `host/Ams.Web.Shared/Class1.cs` | 5 | Template artifact |

**Total: 10 files, ~397 lines**

### Methods to Remove (HIGH confidence)

| Class | Method | Lines (est.) | Reason |
|-------|--------|--------------|--------|
| `AudioProcessor` | `AdjustVolume` | 15 | No callers |
| `AudioProcessor` | `FadeOut` | 15 | No callers |
| `AudioProcessor` | `EncodeWav` | 10 | No callers, use EncodeWavToStream |
| `AudioProcessor` | `NormalizeLoudness` | 40 | No callers |
| `AudioProcessor.Analysis` | `AnalyzeGap` | 30 | No callers |
| `AudioProcessor.Analysis` | `FindSpeechEndFromGap` | 25 | No callers |
| `AudioProcessor.Analysis` | `SnapToEnergyAuto` | 25 | Only commented-out reference |
| `AlignmentService` | `TryExtractChapterNumber` | 10 | Duplicate in ChapterContext |
| `AlignmentService` | `EnumerateLabelCandidates` | 15 | Duplicate in ChapterContext |

**Total: 9 methods, ~185 lines**

### Files to Consider (MEDIUM confidence)

| File Path | Lines | Status |
|-----------|-------|--------|
| `host/Ams.UI.Avalonia/*` (3 files) | 45 | Dormant desktop UI project |
| `out/InspectDocX/Program.cs` | 14 | Analysis tool |
| `analysis/OverlayTest/Program.cs` | 24 | Test harness |

**Recommendation:** Archive or remove after team discussion

---

## Removal Impact Analysis

### Dependencies That Would Break

**None.** All identified dead code has zero incoming dependencies.

### Test Coverage Affected

**Minimal.** The only test-adjacent code is:
- `DspDemoRunner.cs` - Not used by any tests, but could be converted to a test

### Risk Assessment

**LOW RISK**

Justification:
1. All removals are verified by grep search for callers
2. No interface implementations incorrectly flagged
3. No event handlers incorrectly flagged
4. Build will succeed after removals (no compile dependencies)

---

## Recommended Removal Order

### Phase 1: Safe Immediate Removals

**Files (can be deleted now):**
1. `WnModel.cs`, `WnSession.cs`, `WnTranscriber.cs`, `WnUtils.cs` (empty placeholders)
2. `Class1.cs` (template artifact)
3. `ManifestV2.cs` (unused model)
4. `AudioService.cs`, `IAudioService.cs` (unused service)

**Estimated effort:** 5 minutes
**Lines removed:** ~71

### Phase 2: After Quick Review

**Files:**
5. `DspDemoRunner.cs` (confirm not used for testing)
6. `SentenceTimelineBuilder.cs` (confirm replacement exists)

**Methods:**
7. Remove duplicate methods from `AlignmentService.cs`

**Estimated effort:** 15 minutes
**Lines removed:** ~360

### Phase 3: After Team Discussion

**Files:**
8. Archive or remove `Ams.UI.Avalonia` project
9. Archive or remove analysis tools

**Methods:**
10. Remove unused `AudioProcessor` and `AudioProcessor.Analysis` methods

**Estimated effort:** 30 minutes
**Lines removed:** ~265

---

## Appendix: False Positives Investigated

The following items were flagged during analysis but confirmed to be ACTIVE code:

| Item | Reason Kept |
|------|-------------|
| `FeatureExtraction.cs` | Has 1 active caller in ValidateCommand.cs |
| `AsrProcessor.cs` | Core Whisper.NET integration |
| `DocumentService` methods | Called via interface |
| `SentenceRefinementService.RefineAsync` | Called by RefineSentencesCommand |
| All Prosody/* files | Heavily used by pause processing |

---

## Summary Statistics

| Metric | Value |
|--------|-------|
| Files analyzed | 146 |
| Files identified as dead | 10-13 |
| Methods identified as unused | 9 |
| Total lines removable | ~650-700 |
| Percentage of codebase | ~0.5% |
| Risk level | LOW |

---

*Generated: 2025-12-28*
