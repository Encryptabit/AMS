---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs"
access_modifier: "internal"
complexity: 4
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/utility
  - llm/error-handling
---
# AsrEngineConfig::DownloadModelIfMissingAsync
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrEngine.cs`

## Summary
**Ensure a Whisper model file is present locally by reusing an existing file or downloading and saving it to the resolved destination path.**

`DownloadModelIfMissingAsync` resolves a target model file path (explicit `destinationPath` or `<AppContext.BaseDirectory>/models/{GetDefaultModelFileName(type)}`), normalizes it with `Path.GetFullPath`, and short-circuits when the file already exists. On cache hit it logs via `Log.Debug` and returns the existing path; otherwise it ensures the parent directory exists, logs start/finish with `Log.Info`, streams the model from `WhisperGgmlDownloader.Default.GetGgmlModelAsync(type)`, and writes it to disk with exclusive `File.Open(..., FileMode.Create, FileAccess.Write, FileShare.None)`. The method returns the resolved local path of the ready model file.


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

