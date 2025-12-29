# Unused Methods Analysis

Analysis of public/internal methods with no external callers in active projects.

**Reference:** CALLGRAPH-INVENTORY.md (call graph data from D:/Notes)
**Analysis Date:** 2025-12-28

---

## Summary

| Category | Count | Estimated Lines |
|----------|-------|-----------------|
| Unused public methods | 8 | ~180 |
| Unused internal methods | 2 | ~40 |
| Orphaned class methods (entire class unused) | 8 | ~380 |
| **Total** | **18** | **~600** |

---

## By Project

### Ams.Core

#### Definitely Unused (HIGH confidence)

| Class | Method | Visibility | Evidence | Recommendation |
|-------|--------|------------|----------|----------------|
| `AudioProcessor` | `AdjustVolume` | public | No callers found | **Remove** - Dead code |
| `AudioProcessor` | `FadeOut` | public | No callers found | **Remove** - Dead code |
| `AudioProcessor` | `EncodeWav` | public | No callers found | **Remove** - Use `EncodeWavToStream` instead |
| `AudioProcessor` | `NormalizeLoudness` | public | No callers found | **Remove** - Dead code |
| `AudioProcessor.Analysis` | `AnalyzeGap` | public | No callers found (`.AnalyzeGap(` yields 0 matches) | **Remove** - Dead code |
| `AudioProcessor.Analysis` | `FindSpeechEndFromGap` | public | No callers found | **Remove** - Dead code |
| `AudioProcessor.Analysis` | `SnapToEnergyAuto` | public | Only commented-out call in orphaned file | **Remove** - Dead code |
| `AlignmentService` | `TryExtractChapterNumber` | private | In AlignmentService but never called; duplicate exists in ChapterContext | **Remove** - Duplicate |
| `AlignmentService` | `EnumerateLabelCandidates` | private | In AlignmentService but never called; duplicate exists in ChapterContext | **Remove** - Duplicate |

**Estimated lines removable:** ~180

#### Orphaned Classes (All Methods Unused)

These classes are completely orphaned (covered in ORPHANED-FILES.md but listed here for method counts):

| Class | Methods | Total Lines | Recommendation |
|-------|---------|-------------|----------------|
| `DspDemoRunner` | 5 (RunDemo, Rms, NewPlanar, DbToAmp, WriteWavInterleavedFloat32) | 141 | **Remove entire file** |
| `SentenceTimelineBuilder` | 4 (Build, CalculateEnergyTiming, ClampToWindow, ComputeWindowFromScript) | 182 | **Remove entire file** |
| `ManifestV2` | 1 (ResolveStageDirectory) | 19 | **Remove entire file** |
| `AudioService` | 2 (.ctor, WarmAsync) | 16 | **Remove entire file** |
| `IAudioService` | 1 (WarmAsync) | 5 | **Remove entire file** |

**Estimated lines removable:** ~363

---

### Ams.Cli

| Class | Method | Visibility | Evidence | Recommendation |
|-------|--------|------------|----------|----------------|
| *No unused methods found in CLI commands* | | | | |

**Note:** CLI command methods are entry points invoked by System.CommandLine reflection and are not flagged.

---

### Ams.Web.Api

| Class | Method | Visibility | Evidence | Recommendation |
|-------|--------|------------|----------|----------------|
| *No unused methods found* | | | | |

**Note:** Web API endpoints are invoked by ASP.NET routing and are not flagged.

---

## Methods Excluded from Analysis

The following categories were explicitly excluded:

### 1. Interface Implementations
Methods implementing interfaces are called via polymorphism:
- All `I*` interface methods
- Methods with `override` modifier

### 2. Event Handlers
Methods invoked via reflection or delegate binding

### 3. Test Methods
All methods in `Ams.Tests` project

### 4. Entry Points
- `Main` methods
- CLI command handlers (invoked by System.CommandLine)
- ASP.NET endpoints

### 5. Framework Callbacks
- `Dispose`, `DisposeAsync`
- `GetHashCode`, `Equals`, `ToString` overrides

---

## Analysis Details

### AudioProcessor Unused Methods

**AdjustVolume, FadeOut, NormalizeLoudness:**
- These audio manipulation methods exist in `AudioProcessor.cs`
- No grep matches for method invocations found
- Likely implemented early but never integrated into pipeline
- FFmpeg filter graph approach may have replaced these

**EncodeWav:**
- File-based WAV encoder that calls `EncodeToCustomStream`
- `EncodeWavToStream` (memory stream version) is used instead
- Can be removed; callers should use `EncodeWavToStream` and write manually if file output needed

### AudioProcessor.Analysis Unused Methods

**AnalyzeGap:**
- Returns `GapRmsStats` for gap analysis
- Zero invocations found (`.AnalyzeGap(` matches only definition)
- Implemented but never integrated

**FindSpeechEndFromGap:**
- Finds speech end point from silence gap
- Zero invocations found
- May have been replaced by MFA TextGrid approach

**SnapToEnergyAuto:**
- Auto-tuning version of `SnapToEnergy`
- Only reference is commented-out line in orphaned `SentenceTimelineBuilder.cs`
- Safe to remove

### AlignmentService Duplicates

**TryExtractChapterNumber, EnumerateLabelCandidates:**
- Defined in AlignmentService.cs at lines 228 and 246
- Never called from within AlignmentService
- Identical methods exist in ChapterContext.cs (lines 146 and 127) and ARE used
- Remove the duplicates from AlignmentService

---

## Recommendations

### Immediate Removals (Phase 1)
1. Remove `AudioService.cs` and `IAudioService.cs` (entire files unused)
2. Remove duplicate methods from `AlignmentService.cs` (lines 228-260)
3. Remove orphaned class files (DspDemoRunner, SentenceTimelineBuilder, ManifestV2)

**Estimated savings: ~400 lines**

### After Review (Phase 2)
4. Remove unused AudioProcessor methods (AdjustVolume, FadeOut, EncodeWav, NormalizeLoudness)
5. Remove unused AudioProcessor.Analysis methods (AnalyzeGap, FindSpeechEndFromGap, SnapToEnergyAuto)

**Estimated additional savings: ~150 lines**

---

## Verification Commands Used

```bash
# Search for method invocations
grep -r "\.MethodName(" --include="*.cs" .

# Search for method references (may include definitions)
grep -r "MethodName" --include="*.cs" .

# Verify interface implementation
grep -r ": IInterfaceName" --include="*.cs" .
```

---

*Generated: 2025-12-28*
