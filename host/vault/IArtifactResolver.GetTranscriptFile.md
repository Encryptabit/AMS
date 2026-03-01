---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/di
---
# IArtifactResolver::GetTranscriptFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Return a `FileInfo` for the chapter transcript artifact (`align.tx.json`) resolved from the provided context.**

`GetTranscriptFile` is declared on `IArtifactResolver` and implemented in `FileArtifactResolver` as `new(GetChapterArtifactPath(context, "align.tx.json"))`. It does not read or write data; it deterministically maps a `ChapterContext` to the transcript artifact file handle used by higher-level document-slot wiring in constructor setup.


#### [[IArtifactResolver.GetTranscriptFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
FileInfo GetTranscriptFile(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]

