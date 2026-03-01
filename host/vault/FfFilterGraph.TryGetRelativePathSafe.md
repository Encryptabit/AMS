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
  - llm/validation
  - llm/error-handling
---
# FfFilterGraph::TryGetRelativePathSafe
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Attempts to derive a safe relative path between two paths and falls back to `null` when the result is invalid or path computation fails.**

`TryGetRelativePathSafe` wraps `Path.GetRelativePath(basePath, targetPath)` in a defensive `try/catch` and returns a nullable result instead of propagating path exceptions. After computing the relative path, it rejects values containing `:` (e.g., drive-qualified or otherwise unsafe cross-root results) by returning `null`; otherwise it returns the computed relative string. Any failure during path normalization/resolution also yields `null`.


#### [[FfFilterGraph.TryGetRelativePathSafe]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string TryGetRelativePathSafe(string basePath, string targetPath)
```

**Called-by <-**
- [[FfFilterGraph.ResolveFilterAssetPath]]

