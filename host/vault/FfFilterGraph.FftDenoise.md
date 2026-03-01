---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::FftDenoise
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Construct and add an FFT denoise filter from a parameter object with formatted numeric arguments.**

This overload appends an `afftdn` filter node by calling `AddFilter("afftdn", ("nr", ...))` with the noise-reduction parameter serialized for FFmpeg. The implementation is null-tolerant (`parameters ?? new FftDenoiseFilterParams()`) and formats `NoiseReductionDb` using `FormatDouble` before emission. It returns the current `FfFilterGraph` to preserve fluent chaining semantics.


#### [[FfFilterGraph.FftDenoise]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph FftDenoise(FftDenoiseFilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.FormatDouble]]

**Called-by <-**
- [[FfFilterGraph.FftDenoise_2]]

