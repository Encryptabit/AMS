---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# FilterGraphExecutor::BuildOutput
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Returns the accumulated filter output as an `AudioBuffer` when accumulation is enabled, otherwise no output.**

`BuildOutput` is a thin projection over the optional accumulator path, returning `_accumulator?.ToBuffer(_primaryMetadata)`. If execution was configured for streaming/discard modes (or accumulator was never created), it yields `null`; otherwise it materializes an `AudioBuffer` with preserved primary metadata. The method is side-effect free and performs no additional validation.


#### [[FilterGraphExecutor.BuildOutput]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBuffer BuildOutput()
```

**Calls ->**
- [[AudioAccumulator.ToBuffer]]

**Called-by <-**
- [[FfFilterGraphRunner.ExecuteInternal]]

