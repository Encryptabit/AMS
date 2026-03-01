---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/validation
  - llm/error-handling
---
# AudioTreatmentService::TreatChapterAsync
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs`

## Summary
**Start chapter audio treatment using the chapter’s configured roomtone buffer and return the resulting treatment metadata.**

This `TreatChapterAsync` overload is an async entry point that validates `chapter` and non-empty `outputPath`, resolves roomtone from `chapter.Book.Audio.Roomtone`, and fails fast with `InvalidOperationException` when missing (including the expected path in the message). It does not perform treatment logic directly; it delegates to `TreatChapterCoreAsync(chapter, roomtoneBuffer, outputPath, options, cancellationToken)` and returns its awaited `TreatmentResult`. Optional `TreatmentOptions` and cancellation are forwarded unchanged to the core pipeline.


#### [[AudioTreatmentService.TreatChapterAsync]]
##### What it does:
<member name="M:Ams.Core.Audio.AudioTreatmentService.TreatChapterAsync(Ams.Core.Runtime.Chapter.ChapterContext,System.String,Ams.Core.Audio.TreatmentOptions,System.Threading.CancellationToken)">
    <summary>
    Treats a chapter audio by assembling:
    [preroll roomtone] -> [title segment] -> [gap roomtone] -> [content segment] -> [postroll roomtone]
    Uses roomtone from the book's audio context.
    </summary>
    <param name="chapter">The chapter context containing the audio buffer.</param>
    <param name="outputPath">Path for the output treated.wav file.</param>
    <param name="options">Treatment options (timing durations, thresholds).</param>
    <param name="cancellationToken">Cancellation token.</param>
    <returns>Treatment result with timing information.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<AudioTreatmentService.TreatmentResult> TreatChapterAsync(ChapterContext chapter, string outputPath, TreatmentOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[AudioTreatmentService.TreatChapterCoreAsync]]

**Called-by <-**
- [[TreatCommand.Create]]

