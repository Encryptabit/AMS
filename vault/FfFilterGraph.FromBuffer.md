---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 6
fan_out: 0
tags:
  - method
---
# FfFilterGraph::FromBuffer
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.FromBuffer]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.FromBuffer(Ams.Core.Artifacts.AudioBuffer,System.String)">
    <summary>
    Begin a fluent graph with a single <see cref="T:Ams.Core.Artifacts.AudioBuffer"/>.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static FfFilterGraph FromBuffer(AudioBuffer buffer, string label = null)
```

**Called-by <-**
- [[DspCommand.BuildFilterGraph]]
- [[AsrAudioPreparer.DownmixToMono]]
- [[AudioProcessor.DetectSilence]]
- [[AudioProcessor.FadeIn]]
- [[AudioProcessor.Resample]]
- [[AudioProcessor.Trim]]

