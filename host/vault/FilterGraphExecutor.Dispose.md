---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 6
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# FilterGraphExecutor::Dispose
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Releases all filter-executor native allocations and dependent disposable components.**

`Dispose` performs ordered teardown of native and managed executor resources. It frees `_outputFrame` via `av_frame_free`, frees `_graph` via `avfilter_graph_free`, disposes each entry in `_inputs` (releasing per-source native state), and finally disposes `_frameSink` if present. The method uses null checks and local pointer temporaries before free calls to avoid invalid free operations during repeated/partial cleanup states.


#### [[FilterGraphExecutor.Dispose]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Dispose()
```

**Calls ->**
- [[GraphInputState.Dispose]]

