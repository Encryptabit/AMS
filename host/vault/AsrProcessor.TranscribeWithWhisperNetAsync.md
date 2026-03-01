---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/async
  - llm/error-handling
  - llm/utility
---
# AsrProcessor::TranscribeWithWhisperNetAsync
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Executes Whisper transcription and conditionally retries once without DTW timestamps when the first pass appears truncated.**

`TranscribeWithWhisperNetAsync` runs an initial Whisper pass via `RunWhisperPassAsync` and evaluates output coverage with `ShouldRetryWithoutDtw`. If timestamps look healthy, it returns the first `AsrResponse` unchanged. When DTW appears to truncate coverage, it logs a warning (`Log.Warn`) with model/end/audio/coverage diagnostics, clones options with `UseDtwTimestamps = false`, and performs exactly one fallback transcription pass.


#### [[AsrProcessor.TranscribeWithWhisperNetAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<AsrResponse> TranscribeWithWhisperNetAsync(AudioBuffer buffer, AsrOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[Log.Warn]]
- [[AsrProcessor.RunWhisperPassAsync]]
- [[AsrProcessor.ShouldRetryWithoutDtw]]

**Called-by <-**
- [[AsrProcessor.TranscribeBufferInternalAsync]]

