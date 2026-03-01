---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs"
access_modifier: "public"
complexity: 5
fan_in: 5
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrAudioPreparer::PrepareForAsr
**Path**: `Projects/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs`

## Summary
**Convert arbitrary audio buffers into ASR-ready mono 16 kHz audio via conditional downmixing and resampling.**

`PrepareForAsr` normalizes an `AudioBuffer` to ASR-required format (mono, `AudioProcessor.DefaultAsrSampleRate`) with minimal work. It null-checks input, fast-returns the original buffer when already 1-channel at target sample rate, then applies two conditional steps in order: `DownmixToMono` if channels are not 1, and `AudioProcessor.Resample(..., DefaultAsrSampleRate)` if sample rate differs. `DownmixToMono` encapsulates the quality strategy (FFmpeg pan-filter path when available, otherwise averaging fallback), while this method orchestrates the pipeline and returns the resulting buffer instance.


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

