---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
---
# FfFilterGraph::ACompressor
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


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

