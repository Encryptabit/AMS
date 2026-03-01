---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 1
tags:
  - method
---
# FfFilterGraph::WithInput
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.WithInput]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.WithInput(Ams.Core.Artifacts.AudioBuffer,System.String)">
    <summary>
    Register another labeled input buffer (useful for sidechains).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph WithInput(AudioBuffer buffer, string label)
```

**Calls ->**
- [[FfFilterGraph.AddInput]]

