---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfFilterGraph::UseCustomGraph
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Replace the fluent-built filter chain with a user-supplied complete filtergraph string.**

This method switches the graph into full-manual mode by validating `filterGraph`, setting `_customGraphOverride = true`, clearing existing `_clauses`, and storing the provided graph string as the sole clause. It throws `ArgumentException` when the input is null/whitespace, preventing invalid overrides. Returning `this` preserves fluent chaining while bypassing normal helper-driven clause composition.


#### [[FfFilterGraph.UseCustomGraph]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.UseCustomGraph(System.String)">
    <summary>
    Provide the entire filtergraph manually (bypasses fluent clauses).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph UseCustomGraph(string filterGraph)
```

