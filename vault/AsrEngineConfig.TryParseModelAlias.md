---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Asr/AsrEngine.cs"
access_modifier: "internal"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# AsrEngineConfig::TryParseModelAlias
**Path**: `home/cari/repos/AMS/host/Ams.Core/Asr/AsrEngine.cs`


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

