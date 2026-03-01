---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/factory
  - llm/utility
---
# AsrProcessor::ConfigureBuilder
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Creates a processor builder from a factory and forwards configuration to the shared builder-configuration routine.**

This overload is a thin adapter that creates a new `WhisperProcessorBuilder` from the provided `WhisperFactory` (`factory.CreateBuilder()`) and immediately delegates all option application to the other `ConfigureBuilder` overload. It adds no additional branching or configuration logic beyond builder instantiation.


#### [[AsrProcessor.ConfigureBuilder]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static WhisperProcessorBuilder ConfigureBuilder(WhisperFactory factory, AsrOptions options, bool enableTokenTimestamps)
```

**Calls ->**
- [[AsrProcessor.ConfigureBuilder_2]]

**Called-by <-**
- [[AsrProcessor.DetectLanguageInternalAsync]]
- [[AsrProcessor.RunWhisperPassAsync]]

