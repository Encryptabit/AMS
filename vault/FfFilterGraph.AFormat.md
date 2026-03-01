---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 2
tags:
  - method
---
# FfFilterGraph::AFormat
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.AFormat]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.AFormat(System.String,System.String,System.Nullable{System.Int32})">
    <summary>
    Ensure downstream filters see a consistent format/layout.
    Uses libavfilter's <c>aformat</c> (ffmpeg <c>-af aformat</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph AFormat(string sampleFormats = "flt", string channelLayouts = null, int? sampleRate = null)
```

**Calls ->**
- [[FfFilterGraph.AddFilter_2]]
- [[FfFilterGraph.FormatDouble]]

