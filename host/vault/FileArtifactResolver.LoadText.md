---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/error-handling
---
# FileArtifactResolver::LoadText
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Loads text file contents when the target path exists, otherwise returns `null`.**

`LoadText` is an expression-bodied helper that conditionally reads a text artifact from disk. It checks `File.Exists(path)` and returns `File.ReadAllText(path)` when present; otherwise it returns `null`. The method has no explicit validation or exception handling, so read errors propagate.


#### [[FileArtifactResolver.LoadText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string LoadText(string path)
```

**Called-by <-**
- [[FileArtifactResolver.LoadAsrTranscriptText]]

