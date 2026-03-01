---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/di
  - llm/validation
  - llm/error-handling
---
# FfFilterGraphRunner::Stream
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Executes a filter graph over labeled inputs and streams produced frames to a caller-provided sink.**

`Stream` is the streaming execution entry point that routes filter-graph output frames into an injected `IAudioFrameSink` instead of materializing an `AudioBuffer`. It validates `inputs` is non-null/non-empty (`ArgumentException`) and enforces a non-null sink (`ArgumentNullException`), then calls `ExecuteInternal(inputs, filterSpec, FilterExecutionMode.DiscardOutput, sink)`. The `DiscardOutput` mode indicates output is consumed via sink callbacks rather than accumulator return.


#### [[FfFilterGraphRunner.Stream]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Stream(IReadOnlyList<FfFilterGraphRunner.GraphInput> inputs, string filterSpec, FfFilterGraphRunner.IAudioFrameSink sink)
```

**Calls ->**
- [[FfFilterGraphRunner.ExecuteInternal]]

**Called-by <-**
- [[FfFilterGraph.StreamToWave]]

