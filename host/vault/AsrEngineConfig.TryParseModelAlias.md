---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs"
access_modifier: "internal"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrEngineConfig::TryParseModelAlias
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs`

## Summary
**Attempt to parse a model alias into `GgmlType` while providing a defaulted out value on failure.**

`TryParseModelAlias` is a boolean wrapper over `ParseModelAlias` that converts nullable parsing into a `Try*` pattern with an `out` parameter. It calls `ParseModelAlias(value)` and, when successful, assigns the parsed enum to `type` and returns `true`. On failure, it sets `type` to `DefaultModelType` and returns `false`, ensuring the out value is always initialized.


#### [[AsrEngineConfig.TryParseModelAlias]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static bool TryParseModelAlias(string value, out GgmlType type)
```

**Calls ->**
- [[AsrEngineConfig.ParseModelAlias]]

**Called-by <-**
- [[AsrEngineConfig.ResolveModelPathAsync]]

