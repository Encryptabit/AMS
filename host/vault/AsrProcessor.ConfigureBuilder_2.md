---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/validation
  - llm/utility
---
# AsrProcessor::ConfigureBuilder
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Configures a Whisper processor builder with threading, timestamping, language, temperature, and sampling strategy settings from ASR options.**

`ConfigureBuilder` applies `AsrOptions` onto an existing `WhisperProcessorBuilder`: it sets thread count (`options.Threads` or `Environment.ProcessorCount`), enables token timestamps/word splitting when requested, and configures language mode (`WithLanguageDetection` for empty/`auto`, otherwise `WithLanguage`). It conditionally sets temperature, then chooses decoding strategy by enabling beam search with `BeamSize` when `BeamSize > 1`, else greedy sampling with `BestOf` when `BestOf > 1`. The method mutates and returns the same builder, centralizing all runtime decoding/timestamp/language tuning in one place.


#### [[AsrProcessor.ConfigureBuilder_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static WhisperProcessorBuilder ConfigureBuilder(WhisperProcessorBuilder builder, AsrOptions options, bool enableTokenTimestamps)
```

**Called-by <-**
- [[AsrProcessor.ConfigureBuilder]]

