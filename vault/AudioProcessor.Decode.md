---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 2
fan_in: 20
fan_out: 1
tags:
  - method
  - danger/high-fan-in
---
# AudioProcessor::Decode
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs`

> [!danger] High Fan-In (20)
> This method is called by 20 other methods. Changes here have wide impact.


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

