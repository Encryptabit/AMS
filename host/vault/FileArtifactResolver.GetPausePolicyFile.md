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
# FileArtifactResolver::GetPausePolicyFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Provides a `FileInfo` handle to the chapter pause policy JSON file path.**

`GetPausePolicyFile` creates a `FileInfo` for the chapter-scoped pause policy file without performing IO. It builds the path by combining `GetChapterRoot(context.Descriptor)` with the fixed filename `"pause-policy.json"` via `Path.Combine`. The method is a lightweight path materializer that does not validate or check file existence.


#### [[FileArtifactResolver.GetPausePolicyFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo GetPausePolicyFile(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterRoot]]

