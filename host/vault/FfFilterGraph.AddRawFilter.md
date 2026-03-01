---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 4
fan_in: 8
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfFilterGraph::AddRawFilter
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Adds a raw FFmpeg filter entry to the current graph while enforcing graph mutability rules and format pinning behavior.**

`AddRawFilter` is a fluent internal builder method that appends a filter clause using pre-serialized FFmpeg arguments. It guards against invalid mutation by throwing `InvalidOperationException` when `_customGraphOverride` is active, sets `_formatPinned` when `name` is `aformat` (case-insensitive), then adds either `name` or `name=rawArgs` to `_clauses` based on `string.IsNullOrWhiteSpace(rawArgs)` and returns `this`.


#### [[FfFilterGraph.AddRawFilter]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private FfFilterGraph AddRawFilter(string name, string rawArgs)
```

**Called-by <-**
- [[FfFilterGraph.AStats]]
- [[FfFilterGraph.AStats_2]]
- [[FfFilterGraph.EbuR128]]
- [[FfFilterGraph.EbuR128_2]]
- [[FfFilterGraph.NeuralDenoise]]
- [[FfFilterGraph.Resample]]
- [[FfFilterGraph.SilenceRemove]]
- [[FfFilterGraph.SilenceRemove_2]]

