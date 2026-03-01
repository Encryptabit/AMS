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
---
# FfFilterGraph::WithOutputLabel
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Overrides the output pad label used when emitting the final filter graph expression.**

This fluent setter updates the graph’s terminal output label from its default (`"out"`) to a caller-provided value. It validates that `label` is not null/whitespace and throws `ArgumentException` when invalid. On success it writes `_outputLabel` and returns the same builder instance for chaining.


#### [[FfFilterGraph.WithOutputLabel]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.WithOutputLabel(System.String)">
    <summary>
    Override the final output label (defaults to "out").
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph WithOutputLabel(string label)
```

