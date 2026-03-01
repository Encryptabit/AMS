---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "private"
complexity: 4
fan_in: 5
fan_out: 1
tags:
  - method
---
# PolishService::GetChapterBuffer
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`


#### [[PolishService.GetChapterBuffer]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishService.GetChapterBuffer(Ams.Core.Runtime.Chapter.ChapterContextHandle)">
    <summary>
    Resolves the best available chapter audio buffer: corrected.wav > treated.wav > raw.
    Uses direct AudioProcessor.Decode to avoid moving AudioBufferManager cursor.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AudioBuffer GetChapterBuffer(ChapterContextHandle handle)
```

**Calls ->**
- [[AudioProcessor.Decode]]

**Called-by <-**
- [[PolishService.ApplyReplacementAsync]]
- [[PolishService.ApplyRoomtoneOperationAsync]]
- [[PolishService.GeneratePreview]]
- [[PolishService.RevertReplacementAsync]]
- [[PolishService.StageReplacement]]

