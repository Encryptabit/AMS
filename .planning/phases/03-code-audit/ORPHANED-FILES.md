# Orphaned Files Analysis

Analysis of source files with no incoming callers, not serving as entry points, and not being pure configuration/model files.

**Reference:** FILE-INVENTORY.md (146 source files)
**Analysis Date:** 2025-12-28

---

## Summary

| Confidence Level | Count | Lines |
|------------------|-------|-------|
| HIGH (definitely orphaned) | 7 | ~388 |
| MEDIUM (likely orphaned) | 4 | ~287 |
| LOW (needs investigation) | 2 | ~45 |
| **Total** | **13** | **~720** |

---

## Definitely Orphaned (HIGH confidence)

Files with no callers that can be safely removed after final review.

| File | Lines | Last Modified | Reason | Recommendation |
|------|-------|--------------|--------|----------------|
| `host/Ams.Core/Audio/SentenceTimelineBuilder.cs` | 182 | N/A | No callers found for `Build()` method or `SentenceTimelineEntry` type | **Remove** - Timeline building logic replaced by other mechanisms |
| `host/Ams.Core/Pipeline/ManifestV2.cs` | 19 | N/A | Zero references to `ManifestV2` type | **Remove** - Unused manifest format |
| `host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnModel.cs` | 7 | N/A | Empty placeholder class with no implementation | **Remove** - Stale placeholder |
| `host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnSession.cs` | 7 | N/A | Empty placeholder class with no implementation | **Remove** - Stale placeholder |
| `host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnTranscriber.cs` | 7 | N/A | Empty placeholder class with no implementation | **Remove** - Stale placeholder |
| `host/Ams.Core/Services/Integrations/ASR/WhisperNet/WnUtils.cs` | 7 | N/A | Empty placeholder static class with no implementation | **Remove** - Stale placeholder |
| `host/Ams.Web.Shared/Class1.cs` | 4 | N/A | Template placeholder file, zero references | **Remove** - Template artifact |

**Total HIGH confidence lines:** ~233

### Verification Performed

Each file was verified by:
1. Grep search for class/type name across all .cs files
2. Grep search for method names where applicable
3. Namespace search to catch indirect references

**WnModel, WnSession, WnTranscriber, WnUtils:**
- These files exist in `Services/Integrations/ASR/WhisperNet/` folder
- All are empty placeholders (single class declaration, no methods)
- Real Whisper.NET integration is in `AsrProcessor.cs` using `WhisperFactory` and `WhisperProcessor` from the Whisper.NET NuGet package
- These Wn* files were likely scaffolded for future refactoring but never implemented

**SentenceTimelineBuilder:**
- Defines `SentenceTimelineEntry` record and `Build()` method
- No callers for either the type or the method
- Contains 182 lines of sophisticated audio timing logic
- May have been replaced by `PauseMapBuilder` or `PauseDynamicsService`

**ManifestV2:**
- Single record type with `ResolveStageDirectory` method
- No references anywhere in codebase
- Likely superseded by `ChapterContext` and document slot pattern

---

## Likely Orphaned (MEDIUM confidence)

Files that appear unused but serve as standalone tools or dormant features.

| File | Lines | Last Modified | Reason | Investigation Needed |
|------|-------|--------------|--------|---------------------|
| `host/Ams.Core/Audio/DspDemoRunner.cs` | 141 | N/A | Only self-reference found; no production callers | Check if exposed via CLI command |
| `out/InspectDocX/Program.cs` | 14 | N/A | Standalone analysis tool, not referenced by main solution | Verify if still useful |
| `analysis/OverlayTest/Program.cs` | 24 | N/A | Test harness for AudioProcessor.OverlayRoomtone | Verify if still useful |
| `host/Ams.UI.Avalonia/` (3 files) | 45 | N/A | Dormant desktop UI project, no production use | Decide: keep for future or remove |

**Total MEDIUM confidence lines:** ~224

### Notes on MEDIUM Files

**DspDemoRunner.cs:**
- 141 lines of demo/test code for AmsDsp native DSP
- Uses `AmsDsp.Create()` to exercise native code
- May be invocable via CLI but not found in command handlers
- Useful for regression testing native wrapper

**InspectDocX/Program.cs:**
- Simple reflection script to inspect DocX library API
- Not part of main solution but in `out/` directory
- Likely one-off investigation tool

**OverlayTest/Program.cs:**
- 24 lines testing `AudioProcessor.OverlayRoomtone`
- In `analysis/` folder, separate from main solution
- Useful for verifying overlay logic

**Ams.UI.Avalonia:**
- App.axaml.cs (22 lines), MainWindow.axaml.cs (10 lines), Program.cs (13 lines)
- Empty shell for future desktop UI
- MainWindow has no implementation (just constructor)
- Consider archiving rather than removing

---

## Uncertain (LOW confidence)

Files that may have callers via reflection, DI, or runtime discovery.

| File | Lines | Last Modified | Reason | Why Uncertain |
|------|-------|--------------|--------|---------------|
| `host/Ams.Core/Audio/FeatureExtraction.cs` | 564 | N/A | Only 1 caller found in ValidateCommand.cs | May be disabled feature; check if caller is reachable |

**Total LOW confidence lines:** ~564 (but caller exists)

### Notes on LOW Files

**FeatureExtraction.cs:**
- 564 lines of sophisticated breath detection algorithm
- Found 1 caller: `ValidateCommand.cs:968` calls `FeatureExtraction.Detect()`
- This is inside an interactive timing validation session
- Not orphaned, but **may represent disabled/experimental feature**
- Recommend: Keep but document as experimental

---

## Exclusions (Files NOT Considered Orphaned)

The following file types were explicitly excluded from orphan analysis:

### 1. Entry Points
- `host/Ams.Cli/Program.cs` - CLI entry point
- `host/Ams.Web.Api/Program.cs` - Web API entry point
- `host/Ams.Web/Program.cs` - Blazor server entry point
- `host/Ams.Web.Client/Program.cs` - WASM client entry point

### 2. Configuration Files
- `GlobalUsings.cs` files (3 total)
- `AssemblyInfo.cs`
- `ApiJsonSerializerContext.cs` (AOT serialization config)

### 3. Test Files
All 9 files in `Ams.Tests/` - test files are entry points for test runner

### 4. Interface Definitions
10 interface files - called via dependency injection

### 5. Model/DTO Files
27 model files - used via serialization or as type definitions

---

## Recommended Actions

### Immediate Removals (Phase 1)
1. Delete WnModel.cs, WnSession.cs, WnTranscriber.cs, WnUtils.cs (empty placeholders)
2. Delete Class1.cs (template artifact)
3. Delete ManifestV2.cs (unused manifest)

**Estimated savings: ~45 lines, 6 files**

### After Team Review (Phase 2)
4. Delete SentenceTimelineBuilder.cs after confirming replacement logic
5. Consider moving DspDemoRunner.cs to test project if still useful
6. Archive or delete Ams.UI.Avalonia project

**Estimated additional savings: ~350+ lines**

### Keep With Documentation
7. FeatureExtraction.cs - Mark as experimental, has 1 caller

---

## Appendix: Search Commands Used

```bash
# Search for type references
grep -r "TypeName" --include="*.cs" .

# Search for method calls
grep -r "MethodName(" --include="*.cs" .

# Count lines
wc -l filename.cs
```

---

*Generated: 2025-12-28*
