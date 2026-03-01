---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 4
fan_out: 0
tags:
  - method
---
# FfFilterGraph::Custom
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.Custom]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.Custom(System.String)">
    <summary>
    Inject a raw filter clause when fluent helpers are insufficient.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph Custom(string rawClause)
```

**Called-by <-**
- [[AsrAudioPreparer.DownmixToMono]]
- [[AudioProcessor.DetectSilence]]
- [[AudioProcessor.FadeIn]]
- [[AudioProcessor.Trim]]

