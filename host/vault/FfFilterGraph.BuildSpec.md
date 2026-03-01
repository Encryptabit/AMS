---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 4
fan_in: 4
fan_out: 1
tags:
  - method
  - llm/utility
---
# FfFilterGraph::BuildSpec
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Generate the executable filter-spec string for FFmpeg from either a custom override graph or the fluent clause list.**

`BuildSpec` materializes the final FFmpeg filtergraph string from internal state, with a branch for manual override mode. When `_customGraphOverride` is set, it returns the first stored clause (or `string.Empty` if none), bypassing fluent composition. Otherwise it calls `EnsureDefaultFormatClause()`, joins `_clauses` with commas (falling back to `"anull"` when empty), and wraps the chain with input/output labels as `[$"{_inputLabel}"]...[$"{_outputLabel}"]` via string interpolation.


#### [[FfFilterGraph.BuildSpec]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.BuildSpec">
    <summary>
    Build the filter spec string (labels + filter chain).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public string BuildSpec()
```

**Calls ->**
- [[FfFilterGraph.EnsureDefaultFormatClause]]

**Called-by <-**
- [[FfFilterGraph.CaptureLogs]]
- [[FfFilterGraph.RunDiscardingOutput]]
- [[FfFilterGraph.StreamToWave]]
- [[FfFilterGraph.ToBuffer]]

