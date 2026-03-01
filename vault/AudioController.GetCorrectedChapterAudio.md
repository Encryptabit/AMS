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
# AudioController::GetCorrectedChapterAudio
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs`


#### [[AudioController.GetCorrectedChapterAudio]]
##### What it does:
<member name="M:Ams.Workstation.Server.Controllers.AudioController.GetCorrectedChapterAudio(System.String)">
    <summary>
    Serves the corrected chapter audio from disk.
    Falls back to treated.wav, then raw audio if corrected.wav does not exist.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IActionResult GetCorrectedChapterAudio(string chapterName)
```

**Calls ->**
- [[AudioBuffer.ToWavStream]]
- [[AudioController.ServeAudioFile]]

