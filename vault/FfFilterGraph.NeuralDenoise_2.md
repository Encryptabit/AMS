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
# FfFilterGraph::NeuralDenoise
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.NeuralDenoise_2]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.NeuralDenoise(System.String)">
    <summary>
    Neural denoiser (libavfilter <c>arnndn</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph NeuralDenoise(string model = "models/sh.rnnn")
```

**Calls ->**
- [[FfFilterGraph.NeuralDenoise]]

