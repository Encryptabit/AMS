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
# FileArtifactResolver::GetTextGridFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Returns a `FileInfo` reference to the chapter’s TextGrid source file path.**

`GetTextGridFile` is a one-line accessor that wraps the resolved TextGrid path in a `FileInfo` object. It delegates path computation to `GetTextGridPath(context)` and does not perform file existence checks, parsing, or other IO. The method serves as a typed path provider for downstream consumers.


#### [[FileArtifactResolver.GetTextGridFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo GetTextGridFile(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetTextGridPath]]

