---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs"
access_modifier: "private"
complexity: 10
fan_in: 2
fan_out: 7
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# AudioTreatmentService::TreatChapterCoreAsync
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs`

## Summary
**Build and write treated chapter audio from detected speech boundaries plus roomtone padding, then return timing metadata.**

`TreatChapterCoreAsync` is the synchronous core pipeline behind chapter treatment, wrapped in a `Task` return. It checks cancellation at entry, defaults null options, loads `chapter.Audio.Current.Buffer` (throwing if missing), runs silence detection (`AudioProcessor.DetectSilence`) and boundary inference (`FindSpeechBoundaries`), then assembles segment buffers in order: preroll roomtone, optional title, optional title-content gap roomtone, content, and postroll (using `PrepareRoomtoneSegment` and `AudioProcessor.Trim`). It validates that content has positive duration, concatenates with `AudioBuffer.Concat`, ensures the output directory exists, and writes the result via `AudioProcessor.EncodeWav`. Finally, it computes total treated duration from configured paddings and extracted spans and returns `Task.FromResult(new TreatmentResult(...))`.


#### [[AudioTreatmentService.TreatChapterCoreAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<AudioTreatmentService.TreatmentResult> TreatChapterCoreAsync(ChapterContext chapter, AudioBuffer roomtoneBuffer, string outputPath, TreatmentOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[AudioBuffer.Concat_2]]
- [[AudioTreatmentService.FindSpeechBoundaries]]
- [[AudioTreatmentService.PrepareRoomtoneSegment]]
- [[Log.Debug]]
- [[AudioProcessor.DetectSilence]]
- [[AudioProcessor.EncodeWav]]
- [[AudioProcessor.Trim]]

**Called-by <-**
- [[AudioTreatmentService.TreatChapterAsync]]
- [[AudioTreatmentService.TreatChapterAsync_2]]

