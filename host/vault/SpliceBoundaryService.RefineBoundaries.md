---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/SpliceBoundaryService.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# SpliceBoundaryService::RefineBoundaries
**Path**: `Projects/AMS/host/Ams.Core/Audio/SpliceBoundaryService.cs`

## Summary
**Refine rough sentence splice points against nearby silence/energy context while preserving safe fallback behavior when refinement is invalid.**

`RefineBoundaries` validates `chapterBuffer`, resolves `options` to `DefaultOptions`, and derives `bufferDurationSec` from `Length / SampleRate` to constrain search ranges. It refines start and end independently via `RefineBoundary`, using asymmetric windows around the rough cuts (`start: prevEnd or roughStart - margin -> roughStart + 0.1`, `end: roughEnd - 0.1 -> nextStart or roughEnd + margin`, clamped to buffer limits). After both passes, it enforces `refinedStart < refinedEnd`; if violated, it reverts both to rough boundaries and marks both methods as `BoundaryMethod.Original`. It returns a `SpliceBoundaryResult` containing refined positions, refinement methods, and original positions for traceability.


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

