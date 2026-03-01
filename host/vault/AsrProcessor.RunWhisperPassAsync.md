---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 6
tags:
  - method
  - llm/async
  - llm/factory
  - llm/error-handling
  - llm/utility
---
# AsrProcessor::RunWhisperPassAsync
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Performs a single end-to-end Whisper inference pass over audio and materializes segment and token transcription output.**

`RunWhisperPassAsync` executes one Whisper.NET transcription pass by creating factory options, acquiring a pooled `WhisperFactory`, and building a processor configured from `options` (including token timestamp mode). It increments/decrements a shared inflight counter with debug logging in a `try/finally`, converts the `AudioBuffer` to a 16-bit WAV stream at `DefaultAsrSampleRate`, and asynchronously iterates `processor.ProcessAsync(...)` under cancellation. For each segment it appends normalized `AsrSegment` records and token-level data via `AppendTokens`, then returns a new `AsrResponse` using the model file name as version metadata.


#### [[AsrProcessor.RunWhisperPassAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<AsrResponse> RunWhisperPassAsync(AudioBuffer buffer, AsrOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[AudioBuffer.ToWavStream]]
- [[Log.Debug]]
- [[AsrProcessor.AppendTokens]]
- [[AsrProcessor.ConfigureBuilder]]
- [[AsrProcessor.CreateFactoryOptions]]
- [[WhisperFactoryPool.Acquire]]

**Called-by <-**
- [[AsrProcessor.TranscribeWithWhisperNetAsync]]

