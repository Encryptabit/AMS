---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FilterGraphExecutor::ConfigureGraph
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Builds and finalizes the FFmpeg filter-graph topology by connecting executor sources and sink to the parsed filter specification.**

`ConfigureGraph` wires pre-created source and sink filter contexts into the user-provided `_filterSpec` using FFmpeg’s `AVFilterInOut` endpoints. It allocates an input endpoint for `"out"` (bound to `_sink`) and builds the outputs linked list from `_inputs` in reverse order, assigning each node name/context/pad metadata before calling `avfilter_graph_parse_ptr` and `avfilter_graph_config`. Parse/config failures are converted to `InvalidOperationException` with `FfUtils.FormatError(...)`, and a `finally` block reliably frees all `AVFilterInOut` allocations.


#### [[FilterGraphExecutor.ConfigureGraph]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void ConfigureGraph()
```

**Calls ->**
- [[FfUtils.FormatError]]

**Called-by <-**
- [[FilterGraphExecutor..ctor]]

