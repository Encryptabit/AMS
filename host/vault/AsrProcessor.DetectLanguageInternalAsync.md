---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 2
fan_in: 0
fan_out: 4
tags:
  - method
  - llm/async
  - llm/factory
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# AsrProcessor::DetectLanguageInternalAsync
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Runs Whisper language detection on prepared audio samples and returns the detected language with a configured fallback.**

`DetectLanguageInternalAsync` performs cancellation guarding, derives `WhisperFactoryOptions` via `CreateFactoryOptions`, and acquires a pooled `WhisperFactory` through `WhisperFactoryPool.Acquire` for the configured model. It builds a processor with `ConfigureBuilder(..., enableTokenTimestamps: false)`, extracts mono PCM samples with `ExtractMonoSamples`, and calls `DetectLanguageWithProbability` on the processor. If detection returns null/whitespace, it falls back to `options.Language`; factory handle and processor are disposed via `using`/`await using`.


#### [[AsrProcessor.DetectLanguageInternalAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<string> DetectLanguageInternalAsync(AudioBuffer buffer, AsrOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[AsrProcessor.ConfigureBuilder]]
- [[AsrProcessor.CreateFactoryOptions]]
- [[AsrProcessor.ExtractMonoSamples]]
- [[WhisperFactoryPool.Acquire]]

