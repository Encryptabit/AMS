---
namespace: "Ams.Workstation.Server.Controllers"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs"
access_modifier: "private"
complexity: 6
fan_in: 2
fan_out: 0
tags:
  - method
---
# AudioController::ServeAudioFile
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs`


#### [[AudioController.ServeAudioFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IActionResult ServeAudioFile(string filePath)
```

**Called-by <-**
- [[AudioController.GetAudio]]
- [[AudioController.GetCorrectedChapterAudio]]

