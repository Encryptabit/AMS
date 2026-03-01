---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "private"
complexity: 2
fan_in: 3
fan_out: 5
tags:
  - method
---
# PolishService::PersistCorrectedBuffer
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`


#### [[PolishService.PersistCorrectedBuffer]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishService.PersistCorrectedBuffer(Ams.Core.Runtime.Chapter.ChapterContextHandle,Ams.Core.Artifacts.AudioBuffer)">
    <summary>
    Writes the result buffer to {stem}.corrected.wav and flushes the cached
    "corrected" AudioBufferContext so it reloads from disk on next access.
    Also clears any preview buffer.
    Probes the source chapter WAV to preserve its bit depth (e.g. 24-bit audiobook masters).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void PersistCorrectedBuffer(ChapterContextHandle handle, AudioBuffer buffer)
```

**Calls ->**
- [[AudioProcessor.EncodeWav]]
- [[AudioBufferManager.Deallocate]]
- [[AudioBufferManager.Load_2]]
- [[PolishService.ResolveSourceBitDepthOrThrow]]
- [[PreviewBufferService.Clear]]

**Called-by <-**
- [[PolishService.ApplyReplacementAsync]]
- [[PolishService.ApplyRoomtoneOperationAsync]]
- [[PolishService.RevertReplacementAsync]]

