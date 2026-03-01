---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrEngineConfig::ResolveModelPath
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs`

## Summary
**Resolve a required Whisper model path from explicit input or environment and fail fast when unavailable.**

`ResolveModelPath` resolves a Whisper model file path synchronously from two sources in priority order: explicit `optionValue`, then the `AMS_WHISPER_MODEL_PATH` environment variable. For either source it returns `Path.GetFullPath(...)` (trimming env input) and does not verify file existence. If neither source is present/non-whitespace, it throws `InvalidOperationException` with guidance to provide `--model-path` or set the environment variable.


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

