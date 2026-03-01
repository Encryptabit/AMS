---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/factory
---
# FfFilterGraph::FftDenoise
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add an FFT-domain denoise filter to the graph using a single scalar noise-reduction argument.**

This expression-bodied overload is a convenience entry point that wraps `noiseReductionDb` (default `12`) in a `FftDenoiseFilterParams` instance and delegates to `FftDenoise(FftDenoiseFilterParams?)`. It keeps call sites simple while routing actual filter emission to the parameterized overload, which maps the value to FFmpeg `afftdn` args (`nr`).


#### [[FfFilterGraph.FftDenoise_2]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.FftDenoise(System.Double)">
    <summary>
    Frequency-domain denoise (libavfilter <c>afftdn</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph FftDenoise(double noiseReductionDb = 12)
```

**Calls ->**
- [[FfFilterGraph.FftDenoise]]

