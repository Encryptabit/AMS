# Scattered Responsibility Analysis

**Analysis Date:** 2025-12-28
**Reference:** CORE-SUBSYSTEMS.md, CALLGRAPH-INSIGHTS.md

---

## Summary

| Metric | Value |
|--------|-------|
| Scattered patterns found | 7 |
| Files affected | 18 |
| Consolidation opportunities | 8 |
| Estimated impact | MEDIUM |

---

## Scattered Patterns

### Pattern 1: Section Resolution Duplication

**Files involved:**
- `Runtime/Chapter/ChapterContext.cs` (lines 53-161)
- `Services/Alignment/AlignmentService.cs` (lines 228-264)

**What's scattered:**
Both files contain nearly identical logic for:
1. `TryExtractChapterNumber(string label, out int number)` - Regex-based chapter number extraction
2. `EnumerateLabelCandidates()` - Iterating chapter ID and root path for section lookup
3. Section resolution using `SectionLocator.ResolveSectionByTitle()`

**Evidence:**
```csharp
// ChapterContext.cs:146-160
private static bool TryExtractChapterNumber(string label, out int number)
{
    number = 0;
    if (string.IsNullOrWhiteSpace(label)) return false;
    var match = System.Text.RegularExpressions.Regex.Match(label, @"^\s*\d+\s*[_-]\s*(\d+)\b");
    if (match.Success && int.TryParse(match.Groups[1].Value, out number)) return true;
    return false;
}

// AlignmentService.cs:228-244 (identical implementation)
private static bool TryExtractChapterNumber(string label, out int number)
{
    number = 0;
    if (string.IsNullOrWhiteSpace(label)) return false;
    var match = Regex.Match(label, @"^\s*\d+\s*[_-]\s*(\d+)\b");
    if (match.Success && int.TryParse(match.Groups[1].Value, out number)) return true;
    return false;
}
```

**Impact:** Code duplication increases maintenance burden; bugs fixed in one location may not be fixed in the other.

**Consolidation target:** Create `ChapterLabelResolver` utility class in `Runtime/Book/` or `Common/` with:
- `TryExtractChapterNumber()`
- `EnumerateLabelCandidates()`
- Remove duplicates from both ChapterContext and AlignmentService

**Effort:** LOW (2-4 hours)

---

### Pattern 2: ASR Buffer Preparation Split

**Files involved:**
- `Services/AsrService.cs` (lines 26-56)
- `Processors/AsrProcessor.cs` (lines 446-460)

**What's scattered:**
Audio buffer preparation for ASR exists in two places:
1. `AsrService.PrepareForAsr()` - Uses FFmpeg filter graph for downmix
2. `AsrProcessor.NormalizeBuffer()` and `DownmixToMono()` - Manual sample manipulation

**Evidence:**
```csharp
// AsrService.cs:45-56
private static AudioBuffer PrepareForAsr(AudioBuffer buffer)
{
    if (buffer.Channels == 1) return buffer;
    return FfFilterGraph
        .FromBuffer(buffer)
        .Custom(BuildMonoPanClause(buffer.Channels))
        .ToBuffer();
}

// AsrProcessor.cs:462-481
private static AudioBuffer DownmixToMono(AudioBuffer buffer)
{
    if (buffer.Channels == 1) return buffer;
    var mono = new AudioBuffer(1, buffer.SampleRate, buffer.Length);
    for (var i = 0; i < buffer.Length; i++)
    {
        double sum = 0;
        for (var ch = 0; ch < buffer.Channels; ch++)
            sum += buffer.Planar[ch][i];
        mono.Planar[0][i] = (float)(sum / buffer.Channels);
    }
    return mono;
}
```

**Impact:** Two different implementations of mono downmix; one uses FFmpeg (higher quality), one uses simple averaging. Results may differ slightly.

**Consolidation target:** Move buffer normalization to `AudioProcessor` with a single `PrepareForAsr()` method that:
- Uses FFmpeg path when available
- Falls back to simple averaging when FFmpeg unavailable
- Remove duplicate from `AsrProcessor`

**Effort:** MEDIUM (4-8 hours)

---

### Pattern 3: Alignment Service God Class

**Files involved:**
- `Services/Alignment/AlignmentService.cs` (681 lines)

**What's scattered:**
AlignmentService handles four distinct responsibilities:
1. Anchor computation (`ComputeAnchorsAsync`)
2. Transcript index building (`BuildTranscriptIndexAsync`)
3. Transcript hydration (`HydrateTranscriptAsync`)
4. Internal rollup logic (`BuildRollups`, `BuildWordOperations`)

**Evidence:**
The class has 3 public methods but 15+ private helper methods spanning:
- Policy building
- Document building
- Window construction
- Phoneme view building
- Timing computation
- Paragraph/sentence rollups

**Impact:**
- Hard to test individual concerns in isolation
- Changes to one concern may affect others
- File is difficult to navigate at 681 lines

**Consolidation target:** Split into focused services:
1. `AnchorComputationService` - Anchor-specific logic
2. `TranscriptIndexBuilder` - Index building
3. `TranscriptHydrator` - Hydration logic
4. Keep `AlignmentService` as thin facade if needed

**Effort:** HIGH (16-24 hours)

---

### Pattern 4: MFA Artifacts in Wrong Location

**Files involved:**
- `Artifacts/Alignment/MfaChapterContext.cs`
- `Artifacts/Alignment/MfaCommandResult.cs`
- `Application/Mfa/MfaWorkflow.cs`
- `Application/Mfa/MfaService.cs`

**What's scattered:**
MFA-specific artifacts are placed in `Artifacts/Alignment/` instead of with MFA code in `Application/Mfa/`:
- `MfaChapterContext` - Context for MFA operations
- `MfaCommandResult` - Result of MFA commands

Meanwhile, `Application/Mfa/` contains the actual MFA workflow and service.

**Impact:**
- Confusion about where MFA-related code belongs
- Breaks cohesion of MFA subsystem
- Discovery: Looking for MFA code requires checking multiple locations

**Consolidation target:** Move to `Application/Mfa/Models/`:
- `MfaChapterContext.cs`
- `MfaCommandResult.cs`

OR create `Artifacts/Mfa/` folder to maintain artifacts pattern but keep MFA together.

**Effort:** LOW (1-2 hours)

---

### Pattern 5: Document Loading Patterns

**Files involved:**
- `Runtime/Chapter/ChapterDocuments.cs` - Document slot-based lazy loading
- `Runtime/Book/BookDocuments.cs` - Document slot-based lazy loading
- `Processors/DocumentProcessor/*.cs` - Direct parsing and caching

**What's scattered:**
Two patterns for document access:
1. **DocumentSlot pattern** (Runtime): Lazy-loading with slot adapters, persistence
2. **DocumentProcessor pattern** (Processors): Static methods with explicit caching

**Evidence:**
```csharp
// ChapterDocuments - uses DocumentSlot<T>
public TranscriptIndex? Transcript
{
    get => _transcriptSlot.Document;
    set => _transcriptSlot.Document = value;
}

// DocumentProcessor - static parsing
public static async Task<BookParseResult> ParseBookAsync(string bookPath, ...)
{
    var cache = CreateBookCache();
    if (cache?.TryGet(bookPath, out var result) ?? false)
        return result;
    // ... parsing logic
}
```

**Impact:**
- Inconsistent caching strategies
- DocumentProcessor cache is separate from BookCache
- Callers need to know which pattern to use

**Consolidation target:** Unify document access pattern:
- Use DocumentSlot consistently for runtime access
- DocumentProcessor becomes internal implementation detail of BookIndexer/BookParser
- Single cache layer managed by Runtime

**Effort:** HIGH (24+ hours, architectural change)

---

### Pattern 6: Validation Logic Spread

**Files involved:**
- `Services/ValidationService.cs` - High-level validation orchestration
- `Processors/Validation/ValidationReportBuilder.cs` - Report building
- `Validation/ScriptValidator.cs` - Script validation
- `Validation/ValidationModels.cs` - Validation data models
- `Artifacts/Validation/ValidationReportModels.cs` - Report data models

**What's scattered:**
Validation has models in two locations:
1. `Validation/ValidationModels.cs`
2. `Artifacts/Validation/ValidationReportModels.cs`

And logic in three locations:
1. `Services/ValidationService.cs`
2. `Processors/Validation/ValidationReportBuilder.cs`
3. `Validation/ScriptValidator.cs`

**Impact:**
- Finding validation-related code requires checking multiple folders
- Unclear distinction between `Validation/` and `Artifacts/Validation/` and `Processors/Validation/`

**Consolidation target:** Consolidate into single `Validation/` folder:
```
Validation/
  ValidationService.cs
  ValidationReportBuilder.cs
  ScriptValidator.cs
  Models/
    ValidationModels.cs
    ValidationReportModels.cs
```

**Effort:** LOW-MEDIUM (4-8 hours)

---

### Pattern 7: Prosody Service vs Static Methods

**Files involved:**
- `Prosody/PauseDynamicsService.cs` - Service class with interface
- `Prosody/PauseCompressionMath.cs` - Static utility methods
- `Prosody/PauseMapBuilder.cs` - Static-like builder
- `Prosody/PauseTimelineApplier.cs` - Static-like applier

**What's scattered:**
Inconsistent patterns in Prosody:
- `PauseDynamicsService` is a proper service with `IPauseDynamicsService`
- Other prosody classes are static/utility-style without interfaces

**Evidence:**
```csharp
// Service pattern
public class PauseDynamicsService : IPauseDynamicsService
{
    public PauseAnalysisReport Analyze(...) { ... }
}

// Utility pattern (no interface, static-like)
public static class PauseCompressionMath
{
    public static double Compress(...) { ... }
}
```

**Impact:**
- Inconsistent testability (service can be mocked, utilities cannot)
- Unclear which pattern to follow for new prosody code

**Consolidation target:** Two options:
1. Make all prosody classes services with interfaces (more testable)
2. Make PauseDynamicsService static and remove interface (if not needed for DI/testing)

Choose based on whether mocking is needed for unit tests.

**Effort:** MEDIUM (8-16 hours)

---

## Consolidation Recommendations

| # | Pattern | Current Location(s) | Target Location | Effort | Priority |
|---|---------|--------------------|-----------------| -------|----------|
| 1 | Section Resolution | ChapterContext, AlignmentService | Common/ChapterLabelResolver | LOW | HIGH |
| 2 | ASR Buffer Prep | AsrService, AsrProcessor | AudioProcessor.PrepareForAsr | MEDIUM | MEDIUM |
| 3 | Alignment God Class | AlignmentService | Split into 3 services | HIGH | MEDIUM |
| 4 | MFA Artifacts Location | Artifacts/Alignment/ | Application/Mfa/Models/ | LOW | LOW |
| 5 | Document Loading | Runtime + Processors | Unified Runtime pattern | HIGH | LOW |
| 6 | Validation Structure | 5 folders | Single Validation/ folder | LOW-MEDIUM | LOW |
| 7 | Prosody Consistency | Mixed patterns | Consistent service or static | MEDIUM | LOW |

---

## Impact Assessment

### Code Duplication Removed
- Pattern 1: ~50 lines
- Pattern 2: ~30 lines
- Total: ~80 lines

### Maintenance Burden Reduced
- Pattern 1: HIGH (bug-prone duplication)
- Pattern 3: HIGH (large file navigation)
- Pattern 5: MEDIUM (confusion about patterns)

### Risk of Changes
- Patterns 1, 4, 6: LOW (simple moves/consolidation)
- Patterns 2, 7: MEDIUM (behavioral changes possible)
- Patterns 3, 5: HIGH (architectural refactoring)

---

## Quick Wins (Implement First)

1. **Pattern 1: Section Resolution** - Extract duplicate methods to shared utility. Clear win, no risk.

2. **Pattern 4: MFA Artifacts** - Simple file move, improves cohesion with zero risk.

3. **Pattern 6: Validation Structure** - Folder reorganization, improves discoverability.

---

*Generated: 2025-12-28*
