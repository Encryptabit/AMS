---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs"
access_modifier: "public"
complexity: 15
fan_in: 3
fan_out: 3
tags:
  - method
  - danger/high-complexity
  - llm/async
  - llm/data-access
  - llm/factory
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrEngineConfig::ResolveModelPathAsync
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.

## Summary
**Asynchronously determine and provision the effective Whisper model file path and model type using explicit input, aliases, environment configuration, and download fallbacks.**

`ResolveModelPathAsync` resolves a Whisper model using an ordered fallback chain: explicit `modelPath` file, `modelAlias`, `AMS_WHISPER_MODEL_PATH`, then auto-download of `DefaultModelType`. For each branch it normalizes to full paths, infers `GgmlType` via `ParseModelAlias`/`TryParseModelAlias` (falling back to default), and returns `(Path, Type)` when an existing file is found. If a selected path is missing, it calls `DownloadModelIfMissingAsync` either to that destination or to the default model cache location, so resolution and acquisition are unified in one async flow.


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

