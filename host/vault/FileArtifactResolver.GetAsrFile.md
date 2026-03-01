---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/factory
---
# FileArtifactResolver::GetAsrFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Returns a `FileInfo` for the chapter ASR JSON artifact location.**

`GetAsrFile` is a lightweight accessor that returns a `FileInfo` constructed from the chapter ASR artifact path. It delegates path resolution to `GetChapterArtifactPath(context, "asr.json")` and performs no file existence checks or IO. The method simply exposes a typed filesystem reference for downstream callers.


#### [[FileArtifactResolver.GetAsrFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo GetAsrFile(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]

