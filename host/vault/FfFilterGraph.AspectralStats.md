---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::AspectralStats
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Construct and add a spectral-statistics analysis filter with validated numeric bounds and string defaults.**

This method builds and appends an `aspectralstats` filter via `AddFilter("aspectralstats", ...)` from a nullable parameter object (`parameters ?? new AspectralStatsFilterParams()`). It enforces a minimum FFT window size (`Math.Max(p.WindowSize, 32)`), formats numeric values with `FormatDouble`/`FormatFraction`, and applies fallback defaults for empty strings (`win_func` -> `"hann"`, `measure` -> `"all"`). The result is a normalized, fully-populated option set emitted as a fluent graph clause.


#### [[FfFilterGraph.AspectralStats]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.AspectralStats(Ams.Core.Services.Integrations.FFmpeg.AspectralStatsFilterParams)">
    <summary>
    Spectral statistics analyzer (libavfilter <c>aspectralstats</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph AspectralStats(AspectralStatsFilterParams parameters = null)
```

**Calls ->**
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.FormatDouble]]
- [[FfFilterGraph.FormatFraction]]

