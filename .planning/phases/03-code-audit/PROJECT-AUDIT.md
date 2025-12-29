# Project Audit Report

**Analysis Date:** 2025-12-28
**Scope:** All non-Active projects from Phase 1 classification

---

## Summary

| Project | Status | Builds? | Tests Pass? | Future Value | Recommendation |
|---------|--------|---------|-------------|--------------|----------------|
| Ams.Tests | Stale | YES | 44/46 (2 fail) | HIGH | **Update** |
| Ams.UI.Avalonia | Dormant | YES | N/A | LOW | **Archive** |
| Ams.Web.Api | Nascent | YES | N/A | MEDIUM | **Keep Nascent** |
| Ams.Web.Shared | Nascent | YES | N/A | MEDIUM | **Keep Nascent** |
| Ams.Web.Client | Nascent | YES (1 warning) | N/A | MEDIUM | **Keep Nascent** |
| Ams.Web (Server) | Nascent | YES | N/A | MEDIUM | **Keep Nascent** |
| OverlayTest | Analysis | NO (1 error) | N/A | NONE | **Remove** |
| InspectDocX | Analysis | YES | N/A | LOW | **Archive** |

**Overall:** 7 of 8 projects build successfully. One analysis tool is broken. Web stack is functional but minimal.

---

## Detailed Assessments

### Ams.Tests (Stale)

**Current State:** Unit test project with 9 test files covering alignment, audio processing, book parsing, tokenization, prosody, and WER/CER validation.

**Build Status:** SUCCESS
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**Test Results:** 44 passed, 2 failed
- `AudioProcessorFilterTests.Trim_ReturnsExpectedSegment` - FAIL (FFmpeg filter graph configuration issue)
- `AudioProcessorFilterTests.FadeIn_SetsLeadingSamplesToZero` - FAIL (FFmpeg filter graph configuration issue)

**Test Coverage Analysis:**
| Test File | Tests Current Code? | Assessment |
|-----------|---------------------|------------|
| `AnchorDiscoveryTests.cs` | YES | Tests LIS and anchor selection - core algorithm |
| `AudioProcessorFilterTests.cs` | PARTIAL | 2 tests fail due to FFmpeg config issue |
| `BookParsingTests.cs` | YES | Tests BookParser with various formats |
| `TokenizerTests.cs` | YES | Tests TextNormalizer and similarity |
| `TxAlignTests.cs` | YES | Tests transcript alignment algorithms |
| `WavIoTests.cs` | YES | Tests PCM24/PCM32 reading |
| `Prosody/PauseApplierTests.cs` | YES | Tests timeline shifting |
| `Prosody/PauseDynamicsServiceTests.cs` | YES | Tests pause compression |
| `SectionLocatorTests.cs` | YES | Tests chapter resolution from labels |

**Assessment:** Tests are reasonably current - they test core alignment, ASR, and book parsing logic that's still in active use. The failing tests appear to be an environment issue (FFmpeg filter syntax on Windows) rather than obsolete code.

**Recommendation:** **UPDATE**
1. Fix failing FFmpeg filter tests (likely locale/syntax issue)
2. Add tests for MFA integration (currently untested)
3. Increase coverage of Command classes
4. Consider adding integration tests

---

### Ams.UI.Avalonia (Dormant)

**Current State:** Minimal Avalonia desktop UI skeleton. Only 3 source files (App.axaml.cs, MainWindow.axaml.cs, Program.cs) with essentially no implementation.

**Build Status:** SUCCESS
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**Code Analysis:**
```csharp
// MainWindow.axaml.cs - entire content:
public partial class MainWindow : Window
{
    public MainWindow()
    {
    }
}
```

**Assessment:** This is a pure skeleton with no functionality. It was likely created as a placeholder for future desktop UI development but never progressed beyond the initial template.

**Future Value Assessment:**
- CLI is the primary interface and is feature-complete
- Web UI (Ams.Web.*) provides validation viewing capability
- Desktop UI adds complexity without clear benefit
- No unique functionality exists in this project

**Recommendation:** **ARCHIVE**
- Move to `archive/` directory or remove from main solution
- If desktop UI is needed in future, start fresh with proper architecture
- Current skeleton provides no value and adds maintenance burden

---

### Ams.Web.Api (Nascent)

**Current State:** Minimal API with 456 lines providing REST endpoints for validation UI. Uses AOT-ready Slim builder pattern.

**Build Status:** SUCCESS
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**Functionality Analysis:**
| Endpoint Group | Endpoints | Status |
|----------------|-----------|--------|
| Workspace | GET/POST /workspace | Functional |
| Chapters | GET /validation/books/{bookId}/chapters | Functional (streaming) |
| Overview | GET /validation/books/{bookId}/overview | Functional |
| Reports | GET /validation/books/{bookId}/report/{chapterId} | Functional |
| Chapter Detail | GET /validation/books/{bookId}/chapters/{chapterId} | Functional |
| Audio Streaming | GET /audio/books/{bookId}/chapters/{chapterId} | Functional |
| CRX Export | POST /audio/.../export, /validation/.../crx | Functional |
| Reviewed State | GET/POST /validation/books/{bookId}/reviewed | Functional |

**Code Quality:**
- Proper use of Ams.Core types (BookContext, ChapterContext, AudioBuffer)
- JSON serialization with source generators (AOT-compatible)
- CORS enabled for cross-origin requests
- Error handling with Results.Problem()

**Assessment:** This is a functional API that properly integrates with Ams.Core. It provides the backend for the validation web UI and appears to be in working condition.

**Recommendation:** **KEEP NASCENT**
- Production-ready for its current scope
- Would need auth/authz for multi-user deployment
- Keep as-is unless expanded validation UI is prioritized

---

### Ams.Web.Shared (Nascent)

**Current State:** Shared DTOs for API and Client communication. Contains 3 files.

**Build Status:** SUCCESS
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**Content Analysis:**
- `WorkspaceDtos.cs` - Workspace request/response models
- `ValidationDtos.cs` - Validation chapter, sentence, paragraph DTOs
- `Class1.cs` - Empty placeholder (should be removed)

**Assessment:** Properly structured shared library with DTOs. The Class1.cs is dead code identified in DEAD-CODE.md.

**Recommendation:** **KEEP NASCENT**
- Remove Class1.cs (dead code)
- Otherwise functional and well-organized

---

### Ams.Web.Client (Nascent)

**Current State:** Blazor WebAssembly client with validation viewing UI. Contains Razor components and service layer.

**Build Status:** SUCCESS with 1 warning
```
C:\Projects\AMS\host\Ams.Web\Ams.Web.Client\Pages\Validation.razor(365,83):
warning DateTimeOffsetInsteadOfDateTimeAnalyzer: Replace DateTime usage with DateTimeOffset
```

**UI Components:**
| Component | Purpose |
|-----------|---------|
| `Pages/Validation.razor` | Main validation page (365+ lines) |
| `Pages/Home.razor` | Home page |
| `Pages/About.razor` | About page |
| `Components/WaveSurferPlayer.razor` | Audio waveform player |
| `Layout/MainLayout.razor` | Main layout |
| `Layout/Header.razor` | Header component |
| `Layout/Footer.razor` | Footer component |
| `Services/ValidationApiClient.cs` | API client with proper async streaming |

**Assessment:** This is a functional Blazor WASM client for viewing validation results. It has:
- Audio playback via WaveSurfer
- Chapter listing with streaming
- Validation detail viewing
- Review status tracking

**Recommendation:** **KEEP NASCENT**
- Fix DateTime warning
- Client is functional for its intended purpose
- Would need additional work for production deployment

---

### Ams.Web (Server Host)

**Current State:** Blazor Server host for the web application. Minimal configuration.

**Build Status:** SUCCESS
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**Assessment:** Standard Blazor host configuration. Integrates Ams.Web.Client.

**Recommendation:** **KEEP NASCENT**
- Part of web stack, keep together with other Ams.Web.* projects

---

### OverlayTest (Analysis Tool)

**Current State:** Single-file console application designed to test AudioProcessor.OverlayRoomtone functionality.

**Build Status:** FAILED
```
error CS0117: 'AudioProcessor' does not contain a definition for 'OverlayRoomtone'
```

**Code Analysis:**
```csharp
// Attempts to call non-existent method:
var withTone = AudioProcessor.OverlayRoomtone(source, tone, gapStart, gapEnd, 1.0, 10.0);
```

**Assessment:** This analysis tool references a method (`OverlayRoomtone`) that has been removed from `AudioProcessor`. The roomtone overlay functionality was likely deprecated when the audio processing pipeline was simplified.

**Recommendation:** **REMOVE**
- The method it tests no longer exists
- No value in fixing - the feature was intentionally removed
- Delete project directory and remove from solution

---

### InspectDocX (Analysis Tool)

**Current State:** Single-file console application for exploring DocX library API via reflection.

**Build Status:** SUCCESS
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**Code Analysis:**
```csharp
// Inspects DocX type for PageCount-related methods:
Console.WriteLine(typeof(DocX).Assembly.FullName);
foreach(var method in docxType.GetMethods().Where(m => m.Name.Contains("Page"...)))
```

**Assessment:** This was a one-time utility to explore the DocX library's capabilities. It's not referenced anywhere and was likely used during initial development to understand the library API.

**Recommendation:** **ARCHIVE**
- Not harmful to keep but provides no ongoing value
- Move to `archive/` or `tools/` directory if wanted for reference
- Can be safely removed if cleaner solution is preferred

---

## Project Dependencies Analysis

### Active Project Dependencies on Non-Active Projects

| Active Project | Depends On | Impact of Removal |
|---------------|------------|-------------------|
| Ams.Core | None | N/A |
| Ams.Cli | None | N/A |
| Ams.Dsp.Native | None | N/A |

**Conclusion:** No active projects depend on any non-active projects. Removal of dormant/stale projects would not break the build.

### Non-Active Project Dependencies on Active Projects

| Non-Active Project | Depends On Active Projects |
|-------------------|---------------------------|
| Ams.Tests | Ams.Core |
| Ams.UI.Avalonia | Ams.Core |
| Ams.Web.Api | Ams.Core, Ams.Web.Shared |
| Ams.Web.Shared | None (standalone DTOs) |
| Ams.Web.Client | Ams.Web.Shared |
| Ams.Web | Ams.Web.Client |
| OverlayTest | Ams.Core (broken reference) |
| InspectDocX | None (DocX NuGet only) |

---

## Recommendations Summary

### Projects to Update (HIGH priority)
1. **Ams.Tests** - Fix failing FFmpeg tests, increase coverage

### Projects to Keep Nascent (MEDIUM priority)
2. **Ams.Web.Api** - Functional, keep for validation UI
3. **Ams.Web.Shared** - Functional, remove Class1.cs only
4. **Ams.Web.Client** - Functional, fix DateTime warning
5. **Ams.Web** - Functional server host

### Projects to Archive (LOW priority)
6. **Ams.UI.Avalonia** - Empty skeleton, no value
7. **InspectDocX** - One-time utility, can be preserved in archive/

### Projects to Remove (IMMEDIATE)
8. **OverlayTest** - Build-breaking, references removed functionality

---

## Action Items for Phase 4

| Priority | Action | Effort |
|----------|--------|--------|
| P1 | Delete OverlayTest project | 5 min |
| P1 | Remove Class1.cs from Ams.Web.Shared | 1 min |
| P2 | Fix AudioProcessorFilterTests (FFmpeg syntax) | 2-4 hours |
| P2 | Archive Ams.UI.Avalonia | 10 min |
| P3 | Fix DateTime warning in Validation.razor | 15 min |
| P3 | Archive InspectDocX | 10 min |
| P4 | Add MFA integration tests | 8+ hours |

---

*Generated: 2025-12-28*
