---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrEngineConfig::IsWhisper
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs`

## Summary
**Return whether the effective ASR engine selection resolves to Whisper.**

`IsWhisper` is a thin predicate wrapper that delegates engine parsing to `Resolve(engineOption)` and compares the result to `AsrEngine.Whisper`. It inherits all normalization/default behavior from `Resolve` (including env fallback and unknown-value fallback). The method itself contains no independent parsing logic.


#### [[AsrEngineConfig.IsWhisper]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static bool IsWhisper(string engineOption = null)
```

**Calls ->**
- [[AsrEngineConfig.Resolve]]

