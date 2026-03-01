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
# IArtifactResolver::GetPausePolicyFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Resolve and return the `FileInfo` for the chapter-level `pause-policy.json` artifact.**

`GetPausePolicyFile` on `IArtifactResolver` is implemented in `FileArtifactResolver` as `new(Path.Combine(GetChapterRoot(context.Descriptor), "pause-policy.json"))`. It is a pure path-construction accessor that returns the chapter-scoped pause-policy file handle without performing reads or writes. `ChapterDocuments` consumes this in constructor wiring for pause-policy slot backing-file metadata.


#### [[IArtifactResolver.GetPausePolicyFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
FileInfo GetPausePolicyFile(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]

