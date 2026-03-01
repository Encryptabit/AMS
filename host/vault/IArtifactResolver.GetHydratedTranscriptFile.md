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
# IArtifactResolver::GetHydratedTranscriptFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Provide the resolved `FileInfo` for the chapter hydrated-transcript artifact file (`align.hydrate.json`).**

`GetHydratedTranscriptFile` is an `IArtifactResolver` file-location accessor implemented in `FileArtifactResolver` as `new(GetChapterArtifactPath(context, "align.hydrate.json"))`. It performs no I/O and simply returns the canonical `FileInfo` for the chapter’s hydrated transcript artifact, which is consumed during constructor-time `DocumentSlot` configuration.


#### [[IArtifactResolver.GetHydratedTranscriptFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
FileInfo GetHydratedTranscriptFile(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]

