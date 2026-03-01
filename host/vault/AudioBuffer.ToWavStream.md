---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/AudioBuffer.cs"
access_modifier: "public"
complexity: 1
fan_in: 7
fan_out: 1
tags:
  - method
  - llm/utility
---
# AudioBuffer::ToWavStream
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/AudioBuffer.cs`

## Summary
**Encode the current `AudioBuffer` into a WAV-formatted `MemoryStream` using optional encode settings.**

`ToWavStream` is a thin convenience wrapper that delegates directly to `AudioProcessor.EncodeWavToStream(this, options)`. It passes the current `AudioBuffer` instance as the source PCM container and forwards optional `AudioEncodeOptions` unchanged. The method contains no additional transformation, buffering, or validation logic.


#### [[AudioBuffer.ToWavStream]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public MemoryStream ToWavStream(AudioEncodeOptions options = null)
```

**Calls ->**
- [[AudioProcessor.EncodeWavToStream]]

**Called-by <-**
- [[GenerateTranscriptCommand.ExportBufferToTempFile]]
- [[AsrProcessor.RunWhisperPassAsync]]
- [[AudioController.GetChapterAudio]]
- [[AudioController.GetChapterRegionAudio]]
- [[AudioController.GetCorrectedChapterAudio]]
- [[AudioController.GetPreviewAudio]]
- [[AudioExportService.ExportSegment]]

