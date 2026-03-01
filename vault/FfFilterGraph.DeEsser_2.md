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
# FfFilterGraph::DeEsser
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.DeEsser_2]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.DeEsser(System.Double,System.Double,System.Double,System.String)">
    <summary>
    Simple de-esser (libavfilter <c>deesser</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph DeEsser(double normalizedFrequency = 0.5, double intensity = 0, double maxReduction = 0.5, string outputMode = "o")
```

**Calls ->**
- [[FfFilterGraph.DeEsser]]

