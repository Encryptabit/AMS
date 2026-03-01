---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/SpliceBoundaryService.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
---
# SpliceBoundaryService::RefineBoundaries
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/SpliceBoundaryService.cs`


#### [[SpliceBoundaryService.RefineBoundaries]]
##### What it does:
<member name="M:Ams.Core.Audio.SpliceBoundaryService.RefineBoundaries(Ams.Core.Artifacts.AudioBuffer,System.Double,System.Double,System.Nullable{System.Double},System.Nullable{System.Double},Ams.Core.Audio.SpliceBoundaryOptions)">
    <summary>
    Refines rough sentence boundaries by finding natural silence gaps near
    the start and end of the target sentence.
    </summary>
    <param name="chapterBuffer">The full chapter audio buffer.</param>
    <param name="roughStartSec">ASR/MFA-derived start time of the sentence.</param>
    <param name="roughEndSec">ASR/MFA-derived end time of the sentence.</param>
    <param name="prevSentenceEndSec">End time of the previous sentence (null if first).</param>
    <param name="nextSentenceStartSec">Start time of the next sentence (null if last).</param>
    <param name="options">Optional refinement thresholds.</param>
    <returns>Refined start/end boundaries with method annotations.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static SpliceBoundaryResult RefineBoundaries(AudioBuffer chapterBuffer, double roughStartSec, double roughEndSec, double? prevSentenceEndSec, double? nextSentenceStartSec, SpliceBoundaryOptions options = null)
```

**Calls ->**
- [[SpliceBoundaryService.RefineBoundary]]

**Called-by <-**
- [[PolishService.StageReplacement]]

