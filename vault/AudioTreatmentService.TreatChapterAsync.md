---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# AudioTreatmentService::TreatChapterAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs`


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

