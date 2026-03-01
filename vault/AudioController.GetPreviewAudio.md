---
namespace: "Ams.Workstation.Server.Controllers"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
---
# AudioController::GetPreviewAudio
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs`


#### [[AudioController.GetPreviewAudio]]
##### What it does:
<member name="M:Ams.Workstation.Server.Controllers.AudioController.GetPreviewAudio">
    <summary>
    Serves the in-memory preview buffer from PreviewBufferService.
    Returns 404 if no preview has been generated yet.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IActionResult GetPreviewAudio()
```

**Calls ->**
- [[AudioBuffer.ToWavStream]]

