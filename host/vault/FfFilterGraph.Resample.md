---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::Resample
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add a raw audio resampling filter node using the requested sample rate from a parameter object.**

This overload normalizes nullable input with `parameters ?? new ResampleFilterParams()` and emits an FFmpeg `aresample` filter using `AddRawFilter`. The raw argument payload is just the sample-rate value interpolated as a string (`$"{p.SampleRate}"`), with no additional option packing. It returns the current `FfFilterGraph` to support fluent chaining.


#### [[FfFilterGraph.Resample]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph Resample(ResampleFilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddRawFilter]]

**Called-by <-**
- [[AudioProcessor.Resample]]

