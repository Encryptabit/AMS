---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 1
fan_in: 3
fan_out: 2
tags:
  - method
  - llm/async
  - llm/utility
  - llm/validation
---
# AsrProcessor::TranscribeBufferInternalAsync
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Applies standard ASR audio preparation and forwards the buffer to the Whisper.NET transcription pipeline.**

`TranscribeBufferInternalAsync` is a thin async orchestration method that first enforces cancellation, normalizes the incoming `AudioBuffer` via `AsrAudioPreparer.PrepareForAsr`, and then delegates the actual model inference to `TranscribeWithWhisperNetAsync`. It does not perform model-path validation itself, relying on upstream public overloads for that. The method centralizes shared preprocessing so all transcription entry points use the same prepared audio path.


#### [[AsrProcessor.TranscribeBufferInternalAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<AsrResponse> TranscribeBufferInternalAsync(AudioBuffer buffer, AsrOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[AsrAudioPreparer.PrepareForAsr]]
- [[AsrProcessor.TranscribeWithWhisperNetAsync]]

**Called-by <-**
- [[AsrProcessor.TranscribeBufferAsync]]
- [[AsrProcessor.TranscribeBufferAsync_2]]
- [[AsrProcessor.TranscribeFileAsync]]

