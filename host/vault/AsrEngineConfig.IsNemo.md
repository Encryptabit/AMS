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
# AsrEngineConfig::IsNemo
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs`

## Summary
**Indicate whether the configured ASR engine resolves to Nemo.**

`IsNemo` is a convenience predicate that calls `Resolve(engineOption)` and checks whether the resolved enum equals `AsrEngine.Nemo`. It relies entirely on `Resolve` for option/environment parsing, normalization, and fallback behavior. The method adds no additional branching or validation.


#### [[AsrEngineConfig.IsNemo]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static bool IsNemo(string engineOption = null)
```

**Calls ->**
- [[AsrEngineConfig.Resolve]]

