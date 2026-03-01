---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Artifacts/AudioBuffer.cs"
access_modifier: "public"
complexity: 1
fan_in: 7
fan_out: 1
tags:
  - method
---
# AudioBuffer::ToWavStream
**Path**: `home/cari/repos/AMS/host/Ams.Core/Artifacts/AudioBuffer.cs`


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

