---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# FilterGraphExecutor::SetupSink
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Creates and configures the FFmpeg buffer sink filter that receives processed audio frames from the graph.**

`SetupSink` initializes the executor’s output sink node using FFmpeg’s `abuffersink` filter. It resolves the filter definition (`avfilter_get_by_name`), allocates a sink context on `_graph` (`avfilter_graph_alloc_filter`), and throws `InvalidOperationException` on either failure. After allocation it applies sink constraints through `ConfigureSinkFormat()` and finalizes the node with `FfUtils.ThrowIfError(avfilter_init_str(_sink, null), ...)`.


#### [[FilterGraphExecutor.SetupSink]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void SetupSink()
```

**Calls ->**
- [[FilterGraphExecutor.ConfigureSinkFormat]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FilterGraphExecutor..ctor]]

