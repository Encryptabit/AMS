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
# FileArtifactResolver::GetChapterArtifactFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Constructs a `FileInfo` handle for an arbitrary chapter artifact path based on a suffix.**

`GetChapterArtifactFile` is a generic artifact-path wrapper that returns `FileInfo` for a chapter artifact identified by `suffix`. It delegates path construction to `GetChapterArtifactPath(context, suffix)` and performs no validation, normalization, or existence checks in this method body. The method provides a typed filesystem reference without triggering IO.


#### [[FileArtifactResolver.GetChapterArtifactFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo GetChapterArtifactFile(ChapterContext context, string suffix)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]

