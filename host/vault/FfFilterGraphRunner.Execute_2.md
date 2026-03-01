---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/validation
  - llm/error-handling
---
# FfFilterGraphRunner::Execute
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Executes a filter graph over multiple labeled inputs in a specified mode without returning an `AudioBuffer`.**

`Execute(IReadOnlyList<GraphInput> inputs, string filterSpec, FilterExecutionMode mode)` is the multi-input, no-return execution path for filter graphs. It validates `inputs` is non-null and non-empty, throwing `ArgumentException` when missing sources, then delegates to `ExecuteInternal(inputs, filterSpec, mode, null)`. Any operational errors are left to downstream execution logic; this method only performs upfront input presence checks.


#### [[FfFilterGraphRunner.Execute_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Execute(IReadOnlyList<FfFilterGraphRunner.GraphInput> inputs, string filterSpec, FfFilterGraphRunner.FilterExecutionMode mode)
```

**Calls ->**
- [[FfFilterGraphRunner.ExecuteInternal]]

**Called-by <-**
- [[FfFilterGraph.CaptureLogs]]
- [[FfFilterGraph.RunDiscardingOutput]]

