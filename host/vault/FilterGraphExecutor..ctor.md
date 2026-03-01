---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 9
fan_in: 0
fan_out: 6
tags:
  - method
  - llm/utility
  - llm/di
  - llm/validation
  - llm/error-handling
---
# FilterGraphExecutor::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Initializes all native graph resources, input state, output format, and optional sink/accumulator needed to execute a filter graph run.**

The `FilterGraphExecutor` constructor bootstraps the full FFmpeg execution state: it calls `FfSession.EnsureFiltersAvailable()`, normalizes empty `filterSpec` to `"anull"`, allocates `_graph` via `avfilter_graph_alloc`, and materializes `_inputs` through `CreateInputs(inputs)`. It validates critical prerequisites by throwing when graph allocation fails or when no inputs are produced, then derives `_primaryMetadata`, `_channels`, and `_sampleRate` from the first input before running `SetupSink()`, `ConfigureGraph()`, and `RefreshOutputFormat()`. It allocates `_outputFrame` (`av_frame_alloc`) with failure checks, initializes the injected sink via `frameSink.Initialize(...)` when present, and otherwise creates an `AudioAccumulator` only for `ReturnAudio` mode.


#### [[FilterGraphExecutor..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FilterGraphExecutor(IReadOnlyList<FfFilterGraphRunner.GraphInput> inputs, string filterSpec, FfFilterGraphRunner.FilterExecutionMode mode, FfFilterGraphRunner.IAudioFrameSink frameSink)
```

**Calls ->**
- [[FilterGraphExecutor.ConfigureGraph]]
- [[FilterGraphExecutor.CreateInputs]]
- [[FilterGraphExecutor.RefreshOutputFormat]]
- [[FilterGraphExecutor.SetupSink]]
- [[IAudioFrameSink.Initialize]]
- [[FfSession.EnsureFiltersAvailable]]

