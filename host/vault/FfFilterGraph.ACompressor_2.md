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
# FfFilterGraph::ACompressor
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add an FFmpeg `acompressor` filter to the graph using direct numeric parameters.**

This expression-bodied overload is a convenience API that packages scalar compressor settings into `ACompressorFilterParams` and delegates to `ACompressor(ACompressorFilterParams?)`. Default values (`-18`, `2`, `10`, `100`, `2`) provide a ready-to-use gentle-compression profile, while keeping all argument formatting and filter assembly centralized in the parameterized overload.


#### [[FfFilterGraph.ACompressor_2]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.ACompressor(System.Double,System.Double,System.Double,System.Double,System.Double)">
    <summary>
    Gentle compressor (libavfilter <c>acompressor</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph ACompressor(double thresholdDb = -18, double ratio = 2, double attackMs = 10, double releaseMs = 100, double makeupDb = 2)
```

**Calls ->**
- [[FfFilterGraph.ACompressor]]

