---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 7
fan_in: 0
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::DynaudNorm
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Construct and add a configurable dynamic normalization filter with validated and conditionally included FFmpeg arguments.**

This method builds a dynamic argument list for FFmpeg `dynaudnorm` and submits it via `AddFilter("dynaudnorm", args)`. It is null-tolerant (`parameters ?? new DynaudNormFilterParams()`), applies lower-bound guards to several fields (`FrameLengthMilliseconds >= 10`, `GaussSize >= 1`, `MaxGain >= 1`, `Compress >= 0`), and serializes numeric values using `FormatDouble`/`FormatFraction` while booleans are emitted as `"1"`/`"0"`. Optional `Channels` and `Curve` arguments are conditionally appended only when non-empty.


#### [[FfFilterGraph.DynaudNorm]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.DynaudNorm(Ams.Core.Services.Integrations.FFmpeg.DynaudNormFilterParams)">
    <summary>
    Dynamic audio normalization (libavfilter <c>dynaudnorm</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph DynaudNorm(DynaudNormFilterParams parameters = null)
```

**Calls ->**
- [[FfFilterGraph.AddFilter_2]]
- [[FfFilterGraph.FormatDouble]]
- [[FfFilterGraph.FormatFraction]]

