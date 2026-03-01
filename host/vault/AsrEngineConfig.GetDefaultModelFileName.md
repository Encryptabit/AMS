---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs"
access_modifier: "internal"
complexity: 8
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrEngineConfig::GetDefaultModelFileName
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs`

## Summary
**Derive the default local GGML model filename for a given Whisper model type.**

`GetDefaultModelFileName` maps a `GgmlType` enum value to a Whisper model suffix through a switch expression (`base`, `small`, `medium`, `large-v1`, `large-v2`, `large-v3`, `large-v3-turbo`). It then formats the canonical GGML filename as `ggml-{suffix}.bin`. Unknown enum values fall back to `"base"`, ensuring a deterministic filename is always returned.


#### [[AsrEngineConfig.GetDefaultModelFileName]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static string GetDefaultModelFileName(GgmlType type)
```

**Called-by <-**
- [[AsrEngineConfig.DownloadModelIfMissingAsync]]

