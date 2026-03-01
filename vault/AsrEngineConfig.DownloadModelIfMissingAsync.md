---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Asr/AsrEngine.cs"
access_modifier: "internal"
complexity: 4
fan_in: 1
fan_out: 3
tags:
  - method
---
# AsrEngineConfig::DownloadModelIfMissingAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Asr/AsrEngine.cs`


#### [[AsrEngineConfig.DownloadModelIfMissingAsync]]
##### What it does:
<member name="M:Ams.Core.Asr.AsrEngineConfig.DownloadModelIfMissingAsync(System.String,Whisper.net.Ggml.GgmlType)">
    <summary>
    Downloads a Whisper GGML model if it does not already exist at the target path.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static Task<string> DownloadModelIfMissingAsync(string destinationPath, GgmlType type)
```

**Calls ->**
- [[AsrEngineConfig.GetDefaultModelFileName]]
- [[Log.Debug]]
- [[Log.Info]]

**Called-by <-**
- [[AsrEngineConfig.ResolveModelPathAsync]]

