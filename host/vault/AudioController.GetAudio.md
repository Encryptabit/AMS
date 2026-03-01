---
namespace: "Ams.Workstation.Server.Controllers"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 1
tags:
  - method
---
# AudioController::GetAudio
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs`


#### [[AudioController.GetAudio]]
##### What it does:
<member name="M:Ams.Workstation.Server.Controllers.AudioController.GetAudio(System.String)">
    <summary>
    Serves an audio file from the specified path.
    </summary>
    <param name="path">The file path (URL-encoded)</param>
    <returns>Audio file stream with appropriate content type</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IActionResult GetAudio(string path)
```

**Calls ->**
- [[AudioController.ServeAudioFile]]

