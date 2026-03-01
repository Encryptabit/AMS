---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrProcessor::IsDtwEffectivelyEnabled
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Evaluates whether DTW timestamping is effectively active and supported for the current ASR configuration.**

`IsDtwEffectivelyEnabled` is an expression-bodied predicate that requires three conditions: `options.UseDtwTimestamps`, `options.EnableWordTimestamps`, and `ResolveDtwPreset(options.ModelPath).HasValue`. By checking preset resolution, it distinguishes requested DTW from actually supported DTW for the selected model. This centralizes DTW eligibility logic for downstream fallback decisions.


#### [[AsrProcessor.IsDtwEffectivelyEnabled]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsDtwEffectivelyEnabled(AsrOptions options)
```

**Calls ->**
- [[AsrProcessor.ResolveDtwPreset]]

**Called-by <-**
- [[AsrProcessor.ShouldRetryWithoutDtw]]

