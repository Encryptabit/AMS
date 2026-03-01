---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 3
tags:
  - method
  - llm/async
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# AsrProcessor::TranscribeFileAsync
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Loads and validates an audio file, decodes it to ASR-ready mono samples, and runs Whisper transcription asynchronously.**

`TranscribeFileAsync` validates inputs by rejecting null/whitespace `audioPath`, throwing `FileNotFoundException` when the file is missing, null-checking `options`, and verifying the model via `EnsureModelPath`. It checks cancellation early, decodes the source audio through `AudioProcessor.Decode` using fixed ASR decode settings (`DefaultAsrSampleRate`, mono channel), then asynchronously delegates to `TranscribeBufferInternalAsync`. The method is a file-oriented wrapper that handles I/O and preprocessing before the shared transcription core.


#### [[AsrProcessor.TranscribeFileAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<AsrResponse> TranscribeFileAsync(string audioPath, AsrOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[AsrProcessor.EnsureModelPath]]
- [[AsrProcessor.TranscribeBufferInternalAsync]]
- [[AudioProcessor.Decode]]

