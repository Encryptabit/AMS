---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs"
access_modifier: "private"
complexity: 12
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioTreatmentService::FindSpeechBoundaries
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs`

## Summary
**Determine robust title and content start/end times from silence structure and fallback heuristics in chapter audio.**

`FindSpeechBoundaries` infers title/content boundaries from detected `silenceIntervals` against total `audioDuration = buffer.Length / buffer.SampleRate`. With no silences, it returns no-title markers `(-1, -1)` and treats the full clip as content `(0, audioDuration)`. Otherwise it sets `titleStart` from the first leading silence (if it starts near zero), seeks the first silence whose duration exceeds `gapThreshold` as the title/content split, and falls back to a weaker heuristic (first post-onset gap >1s with at least 0.3s silence) when no significant gap is found. It sets `contentEnd` to the start of trailing terminal silence (if the last silence ends near audio end), then clamps ordering with `Math.Max` so `titleEnd >= titleStart`, `contentStart >= titleEnd`, and `contentEnd >= contentStart` before returning.


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

