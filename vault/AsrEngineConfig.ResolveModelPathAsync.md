---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Asr/AsrEngine.cs"
access_modifier: "public"
complexity: 15
fan_in: 3
fan_out: 3
tags:
  - method
  - danger/high-complexity
---
# AsrEngineConfig::ResolveModelPathAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Asr/AsrEngine.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.


#### [[AsrEngineConfig.ResolveModelPathAsync]]
##### What it does:
<member name="M:Ams.Core.Asr.AsrEngineConfig.ResolveModelPathAsync(System.String,System.IO.FileInfo)">
    <summary>
    Resolves the Whisper model path with full fallback chain:
    explicit path → model alias → environment variable → auto-download default.
    Downloads the model if it doesn't exist on disk.
    </summary>
    <param name="modelAlias">Optional model alias (e.g. "large-v3", "base").</param>
    <param name="modelPath">Optional explicit path to a .bin/.gguf model file.</param>
    <returns>The resolved model path and inferred model type.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<(string Path, GgmlType Type)> ResolveModelPathAsync(string modelAlias = null, FileInfo modelPath = null)
```

**Calls ->**
- [[AsrEngineConfig.DownloadModelIfMissingAsync]]
- [[AsrEngineConfig.ParseModelAlias]]
- [[AsrEngineConfig.TryParseModelAlias]]

**Called-by <-**
- [[GenerateTranscriptCommand.RunWhisperAsync]]
- [[PickupMatchingService.BuildAsrOptionsAsync]]
- [[PolishVerificationService.BuildAsrOptionsAsync]]

