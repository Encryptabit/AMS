---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 9
tags:
  - method
---
# PolishService::RevertReplacementAsync
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`


#### [[PolishService.RevertReplacementAsync]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishService.RevertReplacementAsync(System.String,System.Threading.CancellationToken)">
    <summary>
    Reverts a previously applied replacement by restoring the original audio segment
    from the undo backup, then persists the result to corrected.wav.
    </summary>
    <param name="replacementId">ID of the replacement to revert.</param>
    <param name="ct">Cancellation token.</param>
    <returns>The restored audio buffer and the negative timing delta.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<(AudioBuffer ResultBuffer, double TimingDeltaSec)> RevertReplacementAsync(string replacementId, CancellationToken ct)
```

**Calls ->**
- [[AudioSpliceService.ReplaceSegment]]
- [[PolishService.FindStagedItem]]
- [[PolishService.GetActiveChapterHandleOrThrow]]
- [[PolishService.GetChapterBuffer]]
- [[PolishService.PersistCorrectedBuffer]]
- [[StagingQueueService.ShiftDownstream]]
- [[StagingQueueService.UpdateStatus]]
- [[UndoService.GetUndoRecord]]
- [[UndoService.LoadOriginalSegment]]

