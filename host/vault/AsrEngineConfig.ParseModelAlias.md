---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs"
access_modifier: "public"
complexity: 14
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrEngineConfig::ParseModelAlias
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs`

## Summary
**Convert a user/model string alias into the corresponding Whisper `GgmlType` enum value when recognized.**

`ParseModelAlias` normalizes an input alias to a `GgmlType?` using a non-throwing parse flow. It returns `null` for null/whitespace input, otherwise trims/lowercases the value and strips common filename prefixes/suffixes (`"ggml-"`, `".bin"`). A switch expression maps known aliases (including `.en` variants and `"large"`/`"large-v1"` aliasing) to concrete `GgmlType` members, and returns `null` for unrecognized tokens.


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

