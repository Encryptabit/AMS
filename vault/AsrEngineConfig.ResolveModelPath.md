---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Asr/AsrEngine.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
---
# AsrEngineConfig::ResolveModelPath
**Path**: `home/cari/repos/AMS/host/Ams.Core/Asr/AsrEngine.cs`


#### [[AsrEngineConfig.ResolveModelPath]]
##### What it does:
<member name="M:Ams.Core.Asr.AsrEngineConfig.ResolveModelPath(System.String)">
    <summary>
    Synchronous model path resolution from explicit value or environment variable.
    Throws if neither is available. Prefer <see cref="M:Ams.Core.Asr.AsrEngineConfig.ResolveModelPathAsync(System.String,System.IO.FileInfo)"/> which
    supports auto-download as a fallback.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string ResolveModelPath(string optionValue)
```

