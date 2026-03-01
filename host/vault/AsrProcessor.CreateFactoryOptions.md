---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 7
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/factory
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# AsrProcessor::CreateFactoryOptions
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Builds safe, model-aware `WhisperFactoryOptions` from ASR settings, including guarded DTW enablement and fallback behavior.**

`CreateFactoryOptions` derives Whisper runtime flags from `AsrOptions`, including whether DTW was requested (`UseDtwTimestamps && EnableWordTimestamps`) and whether it is actually usable by resolving a model-specific preset via `ResolveDtwPreset`. If DTW is requested but no preset is found, it emits `Log.Warn` and disables DTW to avoid downstream native failures. It returns a `WhisperFactoryOptions` populated from GPU/flash settings and sets `UseDtwTimeStamps` only when preset resolution succeeded, with `HeadsPreset` defaulting to `WhisperAlignmentHeadsPreset.None` otherwise.


#### [[AsrProcessor.CreateFactoryOptions]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static WhisperFactoryOptions CreateFactoryOptions(AsrOptions options)
```

**Calls ->**
- [[Log.Warn]]
- [[AsrProcessor.ResolveDtwPreset]]

**Called-by <-**
- [[AsrProcessor.DetectLanguageInternalAsync]]
- [[AsrProcessor.RunWhisperPassAsync]]

