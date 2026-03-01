---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# AudioTreatmentService::TreatChapterAsync
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs`

## Summary
**Load roomtone from a provided file path and run the core chapter treatment pipeline with that buffer.**

This `TreatChapterAsync` overload validates `chapter`, `roomtonePath`, and `outputPath`, then loads roomtone audio from the explicit path using `AudioProcessor.Decode`. It delegates all boundary detection, assembly, and file rendering work to `TreatChapterCoreAsync(chapter, roomtoneBuffer, outputPath, options, cancellationToken)` and returns that awaited result. The method is an async-shaped orchestration entry point for the explicit-roomtone workflow.


#### [[AudioTreatmentService.TreatChapterAsync_2]]
##### What it does:
<member name="M:Ams.Core.Audio.AudioTreatmentService.TreatChapterAsync(Ams.Core.Runtime.Chapter.ChapterContext,System.String,System.String,Ams.Core.Audio.TreatmentOptions,System.Threading.CancellationToken)">
    <summary>
    Treats a chapter audio by assembling:
    [preroll roomtone] -> [title segment] -> [gap roomtone] -> [content segment] -> [postroll roomtone]
    Uses an explicit roomtone file path.
    </summary>
    <param name="chapter">The chapter context containing the audio buffer.</param>
    <param name="roomtonePath">Path to the roomtone.wav file.</param>
    <param name="outputPath">Path for the output treated.wav file.</param>
    <param name="options">Treatment options (timing durations, thresholds).</param>
    <param name="cancellationToken">Cancellation token.</param>
    <returns>Treatment result with timing information.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<AudioTreatmentService.TreatmentResult> TreatChapterAsync(ChapterContext chapter, string roomtonePath, string outputPath, TreatmentOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[AudioTreatmentService.TreatChapterCoreAsync]]
- [[AudioProcessor.Decode]]

**Called-by <-**
- [[TreatCommand.Create]]

