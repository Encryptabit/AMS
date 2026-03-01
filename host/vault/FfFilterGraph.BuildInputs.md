---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfFilterGraph::BuildInputs
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Validate that at least one input is registered and return the input list for filter execution.**

`BuildInputs()` is a private guard/helper that exposes the graph’s registered inputs as `IReadOnlyList<FfFilterGraphRunner.GraphInput>`. It enforces a non-empty precondition by checking `_inputs.Count == 0` and throwing `InvalidOperationException` with a clear message if no source buffers were registered. On success, it returns the backing `_inputs` collection for runner execution calls.


#### [[FfFilterGraph.BuildInputs]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IReadOnlyList<FfFilterGraphRunner.GraphInput> BuildInputs()
```

**Called-by <-**
- [[FfFilterGraph.CaptureLogs]]
- [[FfFilterGraph.RunDiscardingOutput]]
- [[FfFilterGraph.StreamToWave]]
- [[FfFilterGraph.ToBuffer]]

