---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 1
fan_in: 4
fan_out: 3
tags:
  - method
---
# FfFilterGraph::ToBuffer
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.ToBuffer]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.ToBuffer">
    <summary>
    Execute the composed graph and return a new <see cref="T:Ams.Core.Artifacts.AudioBuffer"/>.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBuffer ToBuffer()
```

**Calls ->**
- [[FfFilterGraph.BuildInputs]]
- [[FfFilterGraph.BuildSpec]]
- [[FfFilterGraphRunner.Apply_2]]

**Called-by <-**
- [[AsrAudioPreparer.DownmixToMono]]
- [[AudioProcessor.FadeIn]]
- [[AudioProcessor.Resample]]
- [[AudioProcessor.Trim]]

