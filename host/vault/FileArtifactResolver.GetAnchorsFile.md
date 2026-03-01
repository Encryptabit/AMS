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
# FileArtifactResolver::GetAnchorsFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Provides a `FileInfo` reference to the chapter anchors JSON artifact path.**

`GetAnchorsFile` is a one-line helper that creates a `FileInfo` for the chapter anchors artifact location. It computes the path through `GetChapterArtifactPath(context, "align.anchors.json")` and returns the wrapped handle without reading or writing files. The method performs no validation or existence checks.


#### [[FileArtifactResolver.GetAnchorsFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo GetAnchorsFile(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]

