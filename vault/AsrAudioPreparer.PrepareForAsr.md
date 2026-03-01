---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs"
access_modifier: "public"
complexity: 5
fan_in: 5
fan_out: 2
tags:
  - method
---
# AsrAudioPreparer::PrepareForAsr
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs`


#### [[AsrAudioPreparer.PrepareForAsr]]
##### What it does:
<member name="M:Ams.Core.Audio.AsrAudioPreparer.PrepareForAsr(Ams.Core.Artifacts.AudioBuffer)">
    <summary>
    Prepares an audio buffer for ASR by converting to mono and resampling to 16kHz.
    Uses FFmpeg filter graph when available for high-quality conversion,
    falls back to simple averaging otherwise.
    </summary>
    <param name="buffer">The source audio buffer.</param>
    <returns>A mono 16kHz buffer ready for ASR processing.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer PrepareForAsr(AudioBuffer buffer)
```

**Calls ->**
- [[AsrAudioPreparer.DownmixToMono]]
- [[AudioProcessor.Resample]]

**Called-by <-**
- [[AsrProcessor.TranscribeBufferInternalAsync]]
- [[AsrService.ResolveAsrReadyBuffer]]
- [[PickupMatchingService.MatchPickupCrxAsync]]
- [[PickupMatchingService.MatchSinglePickupAsync]]
- [[PolishVerificationService.RevalidateSegmentAsync]]

