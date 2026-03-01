---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 1
fan_in: 5
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/di
---
# FfFilterGraphRunner::ExecuteInternal
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Constructs and runs a filter-graph executor with an optional frame sink, then returns the executor’s built audio output.**

`ExecuteInternal` is the central execution helper that wraps `new FilterGraphExecutor(inputs, filterSpec, mode, frameSink)` in a `using var` scope to ensure deterministic disposal of FFmpeg-backed resources. It performs a two-step pipeline: `executor.Process()` to run/filter frames, then `executor.BuildOutput()` to materialize any accumulated audio result. The method itself contains no branching or validation logic and relies on callers for precondition checks and result enforcement.


#### [[FfFilterGraphRunner.ExecuteInternal]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AudioBuffer ExecuteInternal(IReadOnlyList<FfFilterGraphRunner.GraphInput> inputs, string filterSpec, FfFilterGraphRunner.FilterExecutionMode mode, FfFilterGraphRunner.IAudioFrameSink frameSink)
```

**Calls ->**
- [[FilterGraphExecutor.BuildOutput]]
- [[FilterGraphExecutor.Process]]

**Called-by <-**
- [[FfFilterGraphRunner.Apply]]
- [[FfFilterGraphRunner.Apply_2]]
- [[FfFilterGraphRunner.Execute]]
- [[FfFilterGraphRunner.Execute_2]]
- [[FfFilterGraphRunner.Stream]]

