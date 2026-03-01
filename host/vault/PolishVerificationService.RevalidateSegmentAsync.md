---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 6
tags:
  - method
---
# PolishVerificationService::RevalidateSegmentAsync
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs`


#### [[PolishVerificationService.RevalidateSegmentAsync]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishVerificationService.RevalidateSegmentAsync(Ams.Core.Artifacts.AudioBuffer,System.Double,System.Double,System.String,System.Threading.CancellationToken)">
    <summary>
    Re-runs ASR on a replaced audio segment and computes similarity against the expected text.
    </summary>
    <param name="chapterBuffer">The full chapter audio buffer (post-replacement).</param>
    <param name="startSec">Start time of the affected segment in seconds.</param>
    <param name="endSec">End time of the affected segment in seconds.</param>
    <param name="expectedText">The book text that the replacement should match.</param>
    <param name="ct">Cancellation token.</param>
    <returns>A <see cref="T:Ams.Workstation.Server.Services.RevalidationResult"/> with similarity score and pass/fail status.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<RevalidationResult> RevalidateSegmentAsync(AudioBuffer chapterBuffer, double startSec, double endSec, string expectedText, CancellationToken ct)
```

**Calls ->**
- [[AsrAudioPreparer.PrepareForAsr]]
- [[LevenshteinMetrics.Similarity_2]]
- [[AsrProcessor.TranscribeBufferAsync]]
- [[AudioProcessor.Trim]]
- [[PolishVerificationService.BuildAsrOptionsAsync]]
- [[PolishVerificationService.ExtractFullText]]

