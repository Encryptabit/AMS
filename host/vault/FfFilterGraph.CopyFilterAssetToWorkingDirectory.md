---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
---
# FfFilterGraph::CopyFilterAssetToWorkingDirectory
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Caches a filter asset in the current working directory and returns the relative cache path for later filter use.**

`CopyFilterAssetToWorkingDirectory` materializes a filter model file into a local cache under `./.ams-dsp-models`. It ensures the cache directory exists, computes a destination by filename, compares `File.GetLastWriteTimeUtc` for source and destination, and only copies with `File.Copy(..., overwrite: true)` when the cached file is missing or stale. It returns a relative path (`.ams-dsp-models/<filename>`) rather than the absolute destination.


#### [[FfFilterGraph.CopyFilterAssetToWorkingDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string CopyFilterAssetToWorkingDirectory(string sourcePath)
```

**Called-by <-**
- [[FfFilterGraph.ResolveFilterAssetPath]]

