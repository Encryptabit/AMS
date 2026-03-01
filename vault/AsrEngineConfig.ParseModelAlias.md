---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Asr/AsrEngine.cs"
access_modifier: "public"
complexity: 14
fan_in: 2
fan_out: 0
tags:
  - method
---
# AsrEngineConfig::ParseModelAlias
**Path**: `home/cari/repos/AMS/host/Ams.Core/Asr/AsrEngine.cs`


#### [[AsrEngineConfig.ParseModelAlias]]
##### What it does:
<member name="M:Ams.Core.Asr.AsrEngineConfig.ParseModelAlias(System.String)">
    <summary>
    Parses a model alias string (e.g. "large-v3", "base.en") to a <see cref="T:Whisper.net.Ggml.GgmlType"/>.
    Returns null if the string is not a recognized alias.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static GgmlType? ParseModelAlias(string value)
```

**Called-by <-**
- [[AsrEngineConfig.ResolveModelPathAsync]]
- [[AsrEngineConfig.TryParseModelAlias]]

