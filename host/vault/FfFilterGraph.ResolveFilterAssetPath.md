---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# FfFilterGraph::ResolveFilterAssetPath
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Resolves a denoise model identifier into either an absolute path, a safe working-directory-relative path, or a copied local cache path usable by the filter pipeline.**

`ResolveFilterAssetPath` normalizes an optional model path into a runnable FFmpeg asset reference with layered fallbacks. It trims `model`, returns `string.Empty` for null/whitespace, and short-circuits rooted paths via `Path.IsPathRooted`. For relative inputs, it normalizes separators, probes `Path.Combine(AppContext.BaseDirectory, normalized)` with `File.Exists`, then prefers a safe relative path from `Directory.GetCurrentDirectory()` using `TryGetRelativePathSafe`; if that fails, it delegates to `CopyFilterAssetToWorkingDirectory(fullPath)`. When no bundled asset is found, it returns the trimmed input unchanged.


#### [[FfFilterGraph.ResolveFilterAssetPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveFilterAssetPath(string model)
```

**Calls ->**
- [[FfFilterGraph.CopyFilterAssetToWorkingDirectory]]
- [[FfFilterGraph.TryGetRelativePathSafe]]

**Called-by <-**
- [[FfFilterGraph.NeuralDenoise]]

