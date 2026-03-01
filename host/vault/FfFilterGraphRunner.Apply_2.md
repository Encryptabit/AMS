---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/validation
  - llm/error-handling
---
# FfFilterGraphRunner::Apply
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Executes a filter graph over one or more labeled input buffers and returns the resulting audio buffer.**

`Apply(IReadOnlyList<GraphInput> inputs, string filterSpec)` is the multi-input execution entry point for filter graphs. It validates that `inputs` is non-null and non-empty, throwing `ArgumentException` when no sources are provided. It then runs `ExecuteInternal(inputs, filterSpec, FilterExecutionMode.ReturnAudio, null)` and requires a produced buffer, otherwise throwing `InvalidOperationException` if execution yields `null`.


#### [[FfFilterGraphRunner.Apply_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer Apply(IReadOnlyList<FfFilterGraphRunner.GraphInput> inputs, string filterSpec)
```

**Calls ->**
- [[FfFilterGraphRunner.ExecuteInternal]]

**Called-by <-**
- [[AudioSpliceService.Crossfade]]
- [[FfFilterGraph.ToBuffer]]

