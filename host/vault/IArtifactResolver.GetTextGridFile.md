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
# IArtifactResolver::GetTextGridFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Return the resolved `FileInfo` pointing to the chapter TextGrid artifact path.**

`GetTextGridFile` is an `IArtifactResolver` accessor implemented in `FileArtifactResolver` as `new(GetTextGridPath(context))`. It performs no I/O and simply materializes the resolved TextGrid artifact location as a `FileInfo`, allowing callers to reference the chapter’s MFA-derived TextGrid backing file. In `ChapterDocuments`, this handle is used during constructor wiring for `DocumentSlot<TextGridDocument>` metadata.


#### [[IArtifactResolver.GetTextGridFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
FileInfo GetTextGridFile(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]

