---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/async
  - llm/validation
  - llm/utility
---
# AsrProcessor::TranscribeBufferAsync
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Prepares an in-memory mono float sample buffer and runs asynchronous ASR transcription with validated model options.**

`TranscribeBufferAsync` validates `options`, enforces model availability via `EnsureModelPath`, and checks cancellation before work starts. It materializes the input `ReadOnlyMemory<float>` into a new mono `AudioBuffer` (channel count `1`, sample rate `AudioProcessor.DefaultAsrSampleRate`) by copying samples into the planar channel. It then asynchronously delegates to `TranscribeBufferInternalAsync` for the actual Whisper pipeline execution.


#### [[AsrProcessor.TranscribeBufferAsync_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<AsrResponse> TranscribeBufferAsync(ReadOnlyMemory<float> monoAudio, AsrOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[AsrProcessor.EnsureModelPath]]
- [[AsrProcessor.TranscribeBufferInternalAsync]]

