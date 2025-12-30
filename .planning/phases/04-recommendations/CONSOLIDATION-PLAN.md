# Consolidation Plan

**Consolidation roadmap for scattered logic with migration paths**

Generated: 2025-12-30
Source: Phase 3 artifacts (SCATTERED-LOGIC.md, RESPONSIBILITY-MAP.md, ISSUES-CATALOGUE.md)

---

## Executive Summary

| Metric | Value |
|--------|-------|
| Scattered patterns identified | 7 |
| Files affected | 18 |
| Code duplication to remove | ~80 lines |
| Total consolidation effort | 40-60 hours |
| Quick wins available | 3 (7-12 hours) |

---

## High Priority Consolidations

### 1. Section Resolution Duplication → ChapterLabelResolver

**Issue ID:** AUD-004
**Effort:** LOW (2-4 hours)
**Priority:** HIGH - Bug-prone duplication

#### Current State

Section resolution logic duplicated in two locations:

**File 1:** `host/Ams.Core/Runtime/Chapter/ChapterContext.cs` (lines 127-161)
```csharp
private static bool TryExtractChapterNumber(string label, out int number)
{
    number = 0;
    if (string.IsNullOrWhiteSpace(label)) return false;
    var match = Regex.Match(label, @"^\s*\d+\s*[_-]\s*(\d+)\b");
    if (match.Success && int.TryParse(match.Groups[1].Value, out number)) return true;
    return false;
}

private IEnumerable<string> EnumerateLabelCandidates()
{
    yield return ChapterId;
    yield return Path.GetFileNameWithoutExtension(_rootPath);
}
```

**File 2:** `host/Ams.Core/Services/Alignment/AlignmentService.cs` (lines 228-264)
- Nearly identical implementations
- Same regex pattern
- Same candidate enumeration logic

#### Target Design

Create utility class: `host/Ams.Core/Common/ChapterLabelResolver.cs`

```csharp
namespace Ams.Core.Common;

/// <summary>
/// Resolves chapter labels to sections and extracts chapter metadata.
/// Consolidates logic previously duplicated in ChapterContext and AlignmentService.
/// </summary>
public static class ChapterLabelResolver
{
    private static readonly Regex ChapterNumberPattern =
        new(@"^\s*\d+\s*[_-]\s*(\d+)\b", RegexOptions.Compiled);

    /// <summary>
    /// Attempts to extract a chapter number from a label string.
    /// </summary>
    /// <param name="label">Chapter label (e.g., "01_03" or "Chapter-5")</param>
    /// <param name="number">Extracted chapter number if successful</param>
    /// <returns>True if a chapter number was extracted</returns>
    public static bool TryExtractChapterNumber(string label, out int number)
    {
        number = 0;
        if (string.IsNullOrWhiteSpace(label)) return false;
        var match = ChapterNumberPattern.Match(label);
        if (match.Success && int.TryParse(match.Groups[1].Value, out number))
            return true;
        return false;
    }

    /// <summary>
    /// Enumerates candidate labels for section lookup.
    /// </summary>
    /// <param name="chapterId">Primary chapter identifier</param>
    /// <param name="rootPath">Root path for fallback filename extraction</param>
    /// <returns>Enumerable of label candidates</returns>
    public static IEnumerable<string> EnumerateLabelCandidates(string chapterId, string? rootPath)
    {
        yield return chapterId;
        if (!string.IsNullOrEmpty(rootPath))
            yield return Path.GetFileNameWithoutExtension(rootPath);
    }

    /// <summary>
    /// Resolves a section from the book index using multiple label candidates.
    /// </summary>
    public static MarkdownSection? ResolveSection(
        BookIndex bookIndex,
        string chapterId,
        string? rootPath)
    {
        foreach (var candidate in EnumerateLabelCandidates(chapterId, rootPath))
        {
            var section = SectionLocator.ResolveSectionByTitle(bookIndex, candidate);
            if (section != null) return section;

            if (TryExtractChapterNumber(candidate, out var number))
            {
                section = SectionLocator.ResolveSectionByNumber(bookIndex, number);
                if (section != null) return section;
            }
        }
        return null;
    }
}
```

#### Migration Steps

1. **Create utility class**
   - Create `host/Ams.Core/Common/` directory if needed
   - Add `ChapterLabelResolver.cs` with consolidated logic
   - Add compiled regex for performance

2. **Update ChapterContext**
   - Remove private `TryExtractChapterNumber` method
   - Remove private `EnumerateLabelCandidates` method
   - Change callers to use `ChapterLabelResolver.ResolveSection()`

3. **Update AlignmentService**
   - Remove private duplicate methods (AUD-012)
   - Change callers to use `ChapterLabelResolver` utility

4. **Verify**
   ```bash
   dotnet build
   dotnet test
   ```

#### Files Modified

| File | Action |
|------|--------|
| `Common/ChapterLabelResolver.cs` | CREATE |
| `Runtime/Chapter/ChapterContext.cs` | EDIT - Remove methods, use utility |
| `Services/Alignment/AlignmentService.cs` | EDIT - Remove methods, use utility |

---

### 2. ASR Buffer Preparation → AsrAudioPreparer

**Issue ID:** AUD-013
**Effort:** MEDIUM (4-8 hours)
**Priority:** HIGH - Inconsistent implementations

#### Current State

Mono downmix exists in two locations with different implementations:

**File 1:** `host/Ams.Core/Services/AsrService.cs` (lines 26-56)
- Uses FFmpeg filter graph for downmix
- Higher quality (proper pan law)

```csharp
private static AudioBuffer PrepareForAsr(AudioBuffer buffer)
{
    if (buffer.Channels == 1) return buffer;
    return FfFilterGraph
        .FromBuffer(buffer)
        .Custom(BuildMonoPanClause(buffer.Channels))
        .ToBuffer();
}
```

**File 2:** `host/Ams.Core/Processors/AsrProcessor.cs` (lines 462-481)
- Uses simple averaging
- Faster but lower quality

```csharp
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

#### Target Design

Create utility: `host/Ams.Core/Audio/AsrAudioPreparer.cs`

```csharp
namespace Ams.Core.Audio;

/// <summary>
/// Prepares audio buffers for ASR processing.
/// Uses FFmpeg filter graph for high-quality mono downmix when available,
/// with fallback to simple averaging.
/// </summary>
public static class AsrAudioPreparer
{
    /// <summary>
    /// Prepares an audio buffer for ASR by ensuring mono format.
    /// Uses FFmpeg filter graph for quality downmix.
    /// </summary>
    public static AudioBuffer PrepareForAsr(AudioBuffer buffer)
    {
        if (buffer.Channels == 1) return buffer;

        return FfFilterGraph
            .FromBuffer(buffer)
            .Custom(BuildMonoPanClause(buffer.Channels))
            .ToBuffer();
    }

    /// <summary>
    /// Builds FFmpeg pan clause for mono downmix.
    /// </summary>
    private static string BuildMonoPanClause(int channels)
    {
        // Implementation from AsrService
    }

    /// <summary>
    /// Simple averaging fallback (for testing or non-FFmpeg environments).
    /// </summary>
    public static AudioBuffer DownmixToMonoSimple(AudioBuffer buffer)
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
}
```

#### Migration Steps

1. **Create utility class**
   - Add `host/Ams.Core/Audio/AsrAudioPreparer.cs`
   - Move `BuildMonoPanClause` from AsrService
   - Add both FFmpeg and simple implementations

2. **Update AsrService**
   - Remove `PrepareForAsr` method
   - Remove `BuildMonoPanClause` method
   - Use `AsrAudioPreparer.PrepareForAsr()`

3. **Update AsrProcessor**
   - Remove `DownmixToMono` method
   - Use `AsrAudioPreparer.PrepareForAsr()` for quality
   - Or keep `DownmixToMonoSimple()` if FFmpeg unavailable

4. **Verify**
   ```bash
   dotnet build
   dotnet test
   # Run ASR pipeline to verify audio quality
   ```

#### Files Modified

| File | Action |
|------|--------|
| `Audio/AsrAudioPreparer.cs` | CREATE |
| `Services/AsrService.cs` | EDIT - Remove methods, use utility |
| `Processors/AsrProcessor.cs` | EDIT - Remove method, use utility |

---

## Medium Priority Relocations

### 3. MFA Artifacts → Application/Mfa/

**Issue ID:** AUD-014
**Effort:** LOW (1-2 hours)
**Priority:** MEDIUM - Cohesion improvement

#### Current State

MFA-specific artifacts are in `Artifacts/Alignment/` instead of with MFA code:

- `host/Ams.Core/Artifacts/Alignment/MfaChapterContext.cs`
- `host/Ams.Core/Artifacts/Alignment/MfaCommandResult.cs`

Meanwhile, actual MFA code is in:
- `host/Ams.Core/Application/Mfa/MfaWorkflow.cs`
- `host/Ams.Core/Application/Mfa/MfaService.cs`

#### Target Design

Move to `host/Ams.Core/Application/Mfa/Models/`:

```
Application/Mfa/
├── MfaWorkflow.cs
├── MfaService.cs
└── Models/
    ├── MfaChapterContext.cs
    └── MfaCommandResult.cs
```

#### Migration Steps

1. **Create Models directory**
   ```bash
   mkdir -p host/Ams.Core/Application/Mfa/Models
   ```

2. **Move files with namespace update**
   - Move `MfaChapterContext.cs` → `Application/Mfa/Models/`
   - Move `MfaCommandResult.cs` → `Application/Mfa/Models/`
   - Update namespace: `Ams.Core.Artifacts.Alignment` → `Ams.Core.Application.Mfa.Models`

3. **Update using statements**
   - Find all files referencing these types
   - Update using statements to new namespace

4. **Verify**
   ```bash
   dotnet build
   ```

#### Files Modified

| File | Action |
|------|--------|
| `Application/Mfa/Models/MfaChapterContext.cs` | MOVE + namespace |
| `Application/Mfa/Models/MfaCommandResult.cs` | MOVE + namespace |
| Various callers | EDIT - Update using |

---

### 4. Validation Files Consolidation

**Issue ID:** AUD-026
**Effort:** MEDIUM (4-8 hours)
**Priority:** MEDIUM - Discoverability

#### Current State

Validation files scattered across:
- `Services/ValidationService.cs`
- `Processors/Validation/ValidationReportBuilder.cs`
- `Validation/ScriptValidator.cs`
- `Validation/ValidationModels.cs`
- `Artifacts/Validation/ValidationReportModels.cs`

#### Target Design

Consolidate to single `Application/Validation/` folder:

```
Application/Validation/
├── ValidationService.cs
├── ValidationReportBuilder.cs
├── ScriptValidator.cs
└── Models/
    ├── ValidationModels.cs
    └── ValidationReportModels.cs
```

#### Migration Steps

1. Create `Application/Validation/` directory structure
2. Move each file to new location with namespace update
3. Update using statements across codebase
4. Verify build

---

## Interface Simplification

### 5. Remove IAudioService

**Issue ID:** AUD-005
**Effort:** SMALL (30 minutes)
**Priority:** HIGH - Empty placeholder

**Status:** HANDLED in PRUNING-PLAN.md Phase 1.4

### 6. Review Questionable Interfaces

**Issue IDs:** AUD-015, AUD-016, AUD-017, AUD-018
**Effort:** SMALL (1-2 hours each)
**Priority:** MEDIUM

#### Interfaces to Review

| Interface | Implementation | Callers | Decision |
|-----------|---------------|---------|----------|
| `IMfaService` | `MfaService` | 0 via DI | **SIMPLIFY** - Use concrete |
| `IBookParser` | `BookParser` | 1 | **REVIEW** - Check if mocked |
| `IBookIndexer` | `BookIndexer` | 1 | **REVIEW** - Check if mocked |
| `IBookCache` | `BookCache` | 1 | **REVIEW** - Check if mocked |

#### Decision Matrix

For each interface:

1. **Check DI registration:** Is it registered in `Program.cs`?
2. **Check test usage:** Is it mocked in tests?
3. **Check extensibility:** Are multiple implementations planned?

**If NO to all:** Remove interface, use concrete class.

#### Migration Path (if removing interface)

1. Find all references to interface
2. Replace with concrete class
3. Remove interface file
4. Update DI registration if applicable
5. Verify build

---

## Summary Table

| # | Item | Current Location | Target Location | Effort | Priority |
|---|------|------------------|-----------------|--------|----------|
| 1 | Section Resolution | ChapterContext + AlignmentService | Common/ChapterLabelResolver | LOW | HIGH |
| 2 | ASR Buffer Prep | AsrService + AsrProcessor | Audio/AsrAudioPreparer | MEDIUM | HIGH |
| 3 | MFA Artifacts | Artifacts/Alignment/ | Application/Mfa/Models/ | LOW | MEDIUM |
| 4 | Validation Files | 5 locations | Application/Validation/ | MEDIUM | MEDIUM |
| 5 | IAudioService | Services/ | DELETE | SMALL | HIGH |
| 6 | IMfaService | Services/ | Review/Remove | SMALL | MEDIUM |
| 7 | IBook* interfaces | Services/ | Review/Keep or Remove | SMALL | LOW |

---

## Dependency Diagram: Before/After

### Before: Section Resolution

```
┌─────────────────┐     ┌──────────────────┐
│ ChapterContext  │     │ AlignmentService │
├─────────────────┤     ├──────────────────┤
│ TryExtract...() │     │ TryExtract...()  │
│ EnumerateLabel..│     │ EnumerateLabel.. │
│ (DUPLICATE)     │     │ (DUPLICATE)      │
└─────────────────┘     └──────────────────┘
```

### After: Section Resolution

```
┌─────────────────┐     ┌──────────────────┐
│ ChapterContext  │     │ AlignmentService │
└────────┬────────┘     └────────┬─────────┘
         │                       │
         └───────────┬───────────┘
                     │
            ┌────────▼────────┐
            │ChapterLabelResolver│
            ├─────────────────┤
            │ TryExtract...() │
            │ EnumerateLabel..│
            │ ResolveSection()│
            │ (SINGLE SOURCE) │
            └─────────────────┘
```

### Before: ASR Buffer Preparation

```
┌─────────────────┐     ┌──────────────────┐
│   AsrService    │     │  AsrProcessor    │
├─────────────────┤     ├──────────────────┤
│ PrepareForAsr() │     │ DownmixToMono()  │
│ (FFmpeg)        │     │ (Simple avg)     │
│ HIGH QUALITY    │     │ LOWER QUALITY    │
└─────────────────┘     └──────────────────┘
```

### After: ASR Buffer Preparation

```
┌─────────────────┐     ┌──────────────────┐
│   AsrService    │     │  AsrProcessor    │
└────────┬────────┘     └────────┬─────────┘
         │                       │
         └───────────┬───────────┘
                     │
            ┌────────▼────────┐
            │ AsrAudioPreparer │
            ├─────────────────┤
            │ PrepareForAsr() │
            │ (FFmpeg first)  │
            │ (Simple fallback│
            │ CONSISTENT)     │
            └─────────────────┘
```

---

## Quick Wins Execution Order

For fastest results, execute in this order:

1. **IAudioService removal** (30 min) - See PRUNING-PLAN.md
2. **ChapterLabelResolver extraction** (2-4 hours) - Eliminates bug-prone duplication
3. **MFA artifacts relocation** (1-2 hours) - Simple move, improves cohesion

**Total quick wins:** ~7 hours
**Lines of duplication removed:** ~50

---

*Generated: 2025-12-30*
*Source: Phase 3 artifacts (SCATTERED-LOGIC.md, RESPONSIBILITY-MAP.md, ISSUES-CATALOGUE.md)*
