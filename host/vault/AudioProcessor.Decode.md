---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 2
fan_in: 20
fan_out: 1
tags:
  - method
  - danger/high-fan-in
  - llm/data-access
  - llm/validation
  - llm/utility
---
# AudioProcessor::Decode
**Path**: `Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs`

> [!danger] High Fan-In (20)
> This method is called by 20 other methods. Changes here have wide impact.

## Summary
**Validates a source path and decodes audio into an `AudioBuffer` using FFmpeg-backed decoding with optional decode parameters.**

`Decode` is a lightweight FFmpeg-facing wrapper that validates `path` with `ArgumentException.ThrowIfNullOrWhiteSpace`, resolves optional arguments (`options ?? new AudioDecodeOptions()`), and delegates decoding to `FfDecoder.Decode(path, effective)`. It does not perform additional preprocessing, caching, or error recovery beyond input validation and default option materialization.


#### [[AudioProcessor.Decode]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer Decode(string path, AudioDecodeOptions options = null)
```

**Calls ->**
- [[FfDecoder.Decode]]

**Called-by <-**
- [[DspCommand.ExecuteFilterChain]]
- [[PipelineCommand.ComputeAudioStats]]
- [[PipelineCommand.RunVerify]]
- [[AudioTreatmentService.TreatChapterAsync_2]]
- [[AsrProcessor.TranscribeFileAsync]]
- [[AudioBufferManager.DefaultLoader]]
- [[BookAudio.LoadRoomtone]]
- [[WavIoTests.ReadFloat32_PreservesValues]]
- [[WavIoTests.ReadPcm16_SucceedsWithOddSizedMetadataChunk]]
- [[WavIoTests.ReadPcm24_ConvertsSamplesToFloat]]
- [[WavIoTests.ReadPcm32_ConvertsSamplesToFloat]]
- [[AudioController.GetChapterRegionAudio]]
- [[AudioController.GetWaveformData]]
- [[PickupMatchingService.MatchPickupCrxAsync]]
- [[PickupMatchingService.MatchSinglePickupAsync]]
- [[PolishService.ApplyReplacementAsync]]
- [[PolishService.ApplyRoomtoneOperationAsync]]
- [[PolishService.GeneratePreview]]
- [[PolishService.GetChapterBuffer]]
- [[UndoService.LoadOriginalSegment]]

