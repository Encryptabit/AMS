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
# FileArtifactResolver::GetHydratedTranscriptFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Returns a `FileInfo` pointing to the chapter’s hydrated transcript JSON artifact.**

`GetHydratedTranscriptFile` is a thin path-wrapper that instantiates `FileInfo` using the canonical hydrated-transcript artifact path. It resolves that path via `GetChapterArtifactPath(context, "align.hydrate.json")` and does not touch the filesystem beyond object creation. No validation or existence checks are performed in this method.


#### [[FileArtifactResolver.GetHydratedTranscriptFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo GetHydratedTranscriptFile(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]

