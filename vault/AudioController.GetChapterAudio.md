---
namespace: "Ams.Workstation.Server.Controllers"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs"
access_modifier: "public"
complexity: 6
fan_in: 0
fan_out: 2
tags:
  - method
---
# AudioController::GetChapterAudio
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs`


#### [[AudioController.GetChapterAudio]]
##### What it does:
<member name="M:Ams.Workstation.Server.Controllers.AudioController.GetChapterAudio(System.String,System.Nullable{System.Double},System.Nullable{System.Double})">
    <summary>
    Gets the audio for a chapter from the workspace's AudioBufferContext.
    Streams WAV data from the loaded AudioBuffer.
    When start/end query params are provided, trims to that segment.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IActionResult GetChapterAudio(string chapterName, double? start = null, double? end = null)
```

**Calls ->**
- [[AudioBuffer.ToWavStream]]
- [[AudioProcessor.Trim]]

