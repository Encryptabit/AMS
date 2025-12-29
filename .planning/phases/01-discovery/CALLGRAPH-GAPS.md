# Call Graph Coverage Gaps

Analysis of which source files lack call graph documentation.

**Reference:** FILE-INVENTORY.md (146 source files)
**Comparison:** CALLGRAPH-INVENTORY.md (132 graphs)

---

## Summary

| Status | Count | Percentage |
|--------|-------|------------|
| **Covered** | 132 | 90.4% |
| **Missing (needs graph)** | 9 | 6.2% |
| **N/A (trivial/config)** | 5 | 3.4% |
| **Total** | 146 | 100% |

---

## Coverage Gap Analysis

### Missing Call Graphs - High Priority

These files contain business logic and need call graphs.

| File | Project | Category | Priority | Reason |
|------|---------|----------|----------|--------|
| DspSessionState.cs | Ams.Cli | Service | High | DSP session tracking - affects audio pipeline |
| DspConfigModels.cs | Ams.Cli | Model | Medium | Config models - may have logic |
| FfResampler.cs | Ams.Core | Integration | **Critical** | FFmpeg resampling - P/Invoke code |
| FilterSpecs.cs | Ams.Core | Model | Medium | Filter specifications |
| AlignmentOptions.cs | Ams.Core | Model | Low | Options DTO |
| DocumentSlotOptions.cs | Ams.Core | Model | Low | Options DTO |
| FragmentTiming.cs | Ams.Core | Model | Low | Timing model |
| StopwordSets.cs | Ams.Core | Config | Low | Static data |
| BookModels.cs | Ams.Core | Model | Low | Data models |

### Missing Call Graphs - N/A (Config/Trivial)

These files don't need call graphs - they're configuration or generated.

| File | Project | Reason |
|------|---------|--------|
| GlobalUsings.cs | Ams.Cli | Global using directives only |
| GlobalUsings.cs | Ams.Core | Global using directives only |
| GlobalUsings.cs | Ams.Tests | Global using directives only |
| AssemblyInfo.cs | Ams.Core | Assembly metadata only |
| Program.cs | Ams.UI.Avalonia | Covered by existing Program.md |

### Missing Call Graphs - Web Stack (Low Priority)

Web projects are nascent - graphs deferred until active development.

| File | Project | Status |
|------|---------|--------|
| Class1.cs | Ams.Web.Shared | Placeholder/empty |
| ValidationDtos.cs | Ams.Web.Shared | DTOs only |
| WorkspaceDtos.cs | Ams.Web.Shared | DTOs only |
| Program.cs | Ams.Web | Entry point only |
| ApiJsonSerializerContext.cs | Ams.Web.Api | Generated/AOT config |
| ChapterSummary.cs | Ams.Web.Api | DTO only |

---

## Gap Analysis by Project

### Ams.Core (7 missing, 89 covered)

| Missing File | Location | Graph Needed? |
|-------------|----------|---------------|
| FfResampler.cs | Services/Integrations/FFmpeg/ | **Yes - Critical** |
| FilterSpecs.cs | Services/Integrations/FFmpeg/ | Maybe |
| AlignmentOptions.cs | Services/Alignment/ | No (options DTO) |
| DocumentSlotOptions.cs | Runtime/Common/ | No (options DTO) |
| FragmentTiming.cs | Artifacts/ | No (model) |
| StopwordSets.cs | Processors/Alignment/Anchors/ | No (static data) |
| BookModels.cs | Runtime/Book/ | No (models) |

**Key gap: FfResampler.cs** - This is part of the FFmpeg integration layer and likely contains P/Invoke code. Should be added to graph coverage and included in Plan 01-03 FFmpeg documentation.

### Ams.Cli (3 missing, 19 covered)

| Missing File | Location | Graph Needed? |
|-------------|----------|---------------|
| DspSessionState.cs | Services/ | Yes |
| DspConfigModels.cs | Models/ | Maybe |
| GlobalUsings.cs | Root | No |

### Ams.Tests (1 missing, 8 covered)

| Missing File | Location | Graph Needed? |
|-------------|----------|---------------|
| GlobalUsings.cs | Root | No |

### Ams.Web.* (6 files, 0-1 covered)

Web stack is nascent - low priority for call graph coverage.

### Analysis Tools (2 files, 0 covered)

InspectDocX and OverlayTest are one-off analysis tools - low priority.

---

## Priority Ranking

### Critical Priority (Must Add)

1. **FfResampler.cs** - FFmpeg resampling with P/Invoke code, affects audio pipeline

### High Priority (Should Add)

2. **DspSessionState.cs** - DSP session tracking, affects audio processing flow

### Medium Priority (Consider Adding)

3. **DspConfigModels.cs** - May have validation logic
4. **FilterSpecs.cs** - Filter specification logic

### Low Priority (Skip for Now)

- All model files (DTOs, options, data models)
- Config files (GlobalUsings, AssemblyInfo, StopwordSets)
- Web stack files (nascent, not in use)
- Analysis tools (one-off utilities)

---

## Recommendations

### Immediate Actions

1. **Generate call graph for FfResampler.cs** - Critical gap in FFmpeg documentation
2. **Add DspSessionState.cs to coverage** - Affects DSP pipeline

### Plan 01-03 Considerations

The FFmpeg/P/Invoke documentation plan should include:
- FfResampler.cs (missing from current graphs)
- All Ff*.cs files need P/Invoke documentation beyond method graphs
- Native.cs and AmsDsp.cs need P/Invoke documentation

### Coverage Target

Current: 90.4% (132/146)
After critical gaps addressed: 91.8% (134/146)
Practical maximum: ~93% (excluding config/trivial files)

---

## Files Grouped by Coverage Status

### ✓ Covered (132 files)

See CALLGRAPH-INVENTORY.md for complete list.

### ✗ Missing - Needs Graph (4 files)

1. `host/Ams.Core/Services/Integrations/FFmpeg/FfResampler.cs`
2. `host/Ams.Core/Services/Integrations/FFmpeg/FilterSpecs.cs`
3. `host/Ams.Cli/Services/DspSessionState.cs`
4. `host/Ams.Cli/Models/DspConfigModels.cs`

### ○ Missing - N/A (10 files)

1. `host/Ams.Cli/GlobalUsings.cs`
2. `host/Ams.Core/GlobalUsings.cs`
3. `host/Ams.Core/AssemblyInfo.cs`
4. `host/Ams.Tests/GlobalUsings.cs`
5. `host/Ams.Core/Services/Alignment/AlignmentOptions.cs`
6. `host/Ams.Core/Runtime/Common/DocumentSlotOptions.cs`
7. `host/Ams.Core/Artifacts/FragmentTiming.cs`
8. `host/Ams.Core/Processors/Alignment/Anchors/StopwordSets.cs`
9. `host/Ams.Core/Runtime/Book/BookModels.cs`
10. All Web stack DTOs and config files

---

*Generated: 2025-12-28*
