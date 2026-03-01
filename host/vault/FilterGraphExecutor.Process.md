---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/di
  - llm/error-handling
---
# FilterGraphExecutor::Process
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Executes filtering by feeding all input frames into the graph, draining remaining output, and completing the optional output sink.**

`Process` drives the end-to-end frame push/pull loop for the configured filter graph executor. It iterates all `_inputs`, feeds each via `SendAllFrames(input)`, then signals per-source EOF with `av_buffersrc_add_frame(input.Source, null)` guarded by `FfUtils.ThrowIfError`. After all sources are closed, it performs a final drain (`Drain(final: true)`) and invokes `_frameSink?.Complete()` to finalize sink-side streaming.


#### [[FilterGraphExecutor.Process]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Process()
```

**Calls ->**
- [[FilterGraphExecutor.Drain]]
- [[FilterGraphExecutor.SendAllFrames]]
- [[IAudioFrameSink.Complete]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfFilterGraphRunner.ExecuteInternal]]

