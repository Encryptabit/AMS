---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs"
access_modifier: "private"
complexity: 10
fan_in: 2
fan_out: 7
tags:
  - method
---
# AudioTreatmentService::TreatChapterCoreAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs`


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

