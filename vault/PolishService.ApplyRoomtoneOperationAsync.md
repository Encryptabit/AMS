---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "public"
complexity: 5
fan_in: 0
fan_out: 9
tags:
  - method
---
# PolishService::ApplyRoomtoneOperationAsync
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`


#### [[PolishService.ApplyRoomtoneOperationAsync]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishService.ApplyRoomtoneOperationAsync(Ams.Workstation.Server.Models.RoomtoneRequest,System.String,System.Threading.CancellationToken)">
    <summary>
    Applies a roomtone editing operation (insert, replace, or delete) to the current
    chapter audio at the specified region. Backs up the original segment via UndoService,
    then persists the result as corrected.wav.
    </summary>
    <param name="request">The roomtone operation parameters.</param>
    <param name="roomtoneFilePath">Path to the roomtone WAV file (used for Insert/Replace).</param>
    <param name="ct">Cancellation token.</param>
    <returns>The resulting audio buffer after the operation.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<AudioBuffer> ApplyRoomtoneOperationAsync(RoomtoneRequest request, string roomtoneFilePath, CancellationToken ct)
```

**Calls ->**
- [[AudioSpliceService.DeleteRegion]]
- [[AudioSpliceService.GenerateRoomtoneFill]]
- [[AudioSpliceService.InsertAtPoint]]
- [[AudioSpliceService.ReplaceSegment]]
- [[AudioProcessor.Decode]]
- [[PolishService.GetActiveChapterHandleOrThrow]]
- [[PolishService.GetChapterBuffer]]
- [[PolishService.PersistCorrectedBuffer]]
- [[UndoService.SaveOriginalSegment]]

