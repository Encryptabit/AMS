---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 3
fan_in: 12
fan_out: 3
tags:
  - method
  - danger/high-fan-in
---
# AudioProcessor::Trim
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs`

> [!danger] High Fan-In (12)
> This method is called by 12 other methods. Changes here have wide impact.


#### [[AudioProcessor.Trim]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer Trim(AudioBuffer buffer, TimeSpan start, TimeSpan? end = null)
```

**Calls ->**
- [[FfFilterGraph.Custom]]
- [[FfFilterGraph.FromBuffer]]
- [[FfFilterGraph.ToBuffer]]

**Called-by <-**
- [[AudioSpliceService.DeleteRegion]]
- [[AudioSpliceService.GenerateRoomtoneFill]]
- [[AudioSpliceService.InsertAtPoint]]
- [[AudioSpliceService.ReplaceSegment]]
- [[AudioTreatmentService.TreatChapterCoreAsync]]
- [[SpliceBoundaryService.RefineBoundary]]
- [[AudioProcessorFilterTests.Trim_ReturnsExpectedSegment]]
- [[AudioController.GetChapterAudio]]
- [[AudioExportService.ExportSegment]]
- [[PolishService.TrimPickupForReplacement]]
- [[PolishVerificationService.RevalidateSegmentAsync]]
- [[UndoService.SaveOriginalSegment]]

