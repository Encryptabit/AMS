---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# FileArtifactResolver::EnsureDirectory
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Ensures the parent directory for a target file path exists before writing files.**

`EnsureDirectory` derives the parent directory from a target file path using `Path.GetDirectoryName(filePath)`. If a non-empty directory is present, it creates it via `Directory.CreateDirectory(directory)`, which is idempotent for existing paths. The method intentionally no-ops when the path has no directory component.


#### [[FileArtifactResolver.EnsureDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void EnsureDirectory(string filePath)
```

**Called-by <-**
- [[FileArtifactResolver.SaveAsrTranscriptText]]
- [[FileArtifactResolver.SaveBookIndex]]
- [[FileArtifactResolver.SaveJson]]
- [[FileArtifactResolver.SavePauseAdjustments]]

