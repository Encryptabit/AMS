---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs"
access_modifier: "private"
complexity: 12
fan_in: 1
fan_out: 0
tags:
  - method
---
# AudioTreatmentService::FindSpeechBoundaries
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs`


#### [[AudioTreatmentService.FindSpeechBoundaries]]
##### What it does:
<member name="M:Ams.Core.Audio.AudioTreatmentService.FindSpeechBoundaries(Ams.Core.Artifacts.AudioBuffer,System.Collections.Generic.IReadOnlyList{Ams.Core.Processors.SilenceInterval},System.Double)">
    <summary>
    Finds speech boundaries: title start/end and content start/end.
    Title ends when there's a significant silence gap (>threshold).
    Content starts after that gap and ends at the final speech offset.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (double TitleStart, double TitleEnd, double ContentStart, double ContentEnd) FindSpeechBoundaries(AudioBuffer buffer, IReadOnlyList<SilenceInterval> silenceIntervals, double gapThreshold)
```

**Called-by <-**
- [[AudioTreatmentService.TreatChapterCoreAsync]]

