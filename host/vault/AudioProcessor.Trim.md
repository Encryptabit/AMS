---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 3
fan_in: 12
fan_out: 3
tags:
  - method
  - danger/high-fan-in
  - llm/utility
  - llm/validation
  - llm/data-access
---
# AudioProcessor::Trim
**Path**: `Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs`

> [!danger] High Fan-In (12)
> This method is called by 12 other methods. Changes here have wide impact.

## Summary
**Returns a new audio buffer containing a trimmed segment between start and optional end times with reset presentation timestamps.**

`Trim` extracts a time slice from an `AudioBuffer` by validating `buffer`, clamping `start` to non-negative seconds, and building an FFmpeg `atrim` filter (with optional `end`) followed by `asetpts=PTS-STARTPTS` to rebase timestamps to zero. It uses `FfFilterGraph.FromBuffer(buffer).Custom(filter).ToBuffer()` to execute the transform and return a new buffer. When `end` is provided, it enforces `end >= start` via `Math.Max(startSeconds, end.Value.TotalSeconds)`.


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

