---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 10
tags:
  - method
---
# PolishService::ApplyReplacementAsync
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`


#### [[PolishService.ApplyReplacementAsync]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishService.ApplyReplacementAsync(System.String,System.Threading.CancellationToken)">
    <summary>
    Applies a staged replacement: backs up the original segment, splices in
    the pickup audio with crossfade, writes the result to corrected.wav,
    and records the timing delta.
    </summary>
    <param name="replacementId">ID of the staged replacement to apply.</param>
    <param name="ct">Cancellation token.</param>
    <returns>The spliced audio buffer and the timing delta (positive = longer, negative = shorter).</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<(AudioBuffer ResultBuffer, double TimingDeltaSec)> ApplyReplacementAsync(string replacementId, CancellationToken ct)
```

**Calls ->**
- [[AudioSpliceService.ReplaceSegment]]
- [[AudioProcessor.Decode]]
- [[PolishService.FindStagedItem]]
- [[PolishService.GetActiveChapterHandleOrThrow]]
- [[PolishService.GetChapterBuffer]]
- [[PolishService.PersistCorrectedBuffer]]
- [[PolishService.TrimPickupForReplacement]]
- [[StagingQueueService.ShiftDownstream]]
- [[StagingQueueService.UpdateStatus]]
- [[UndoService.SaveOriginalSegment]]

