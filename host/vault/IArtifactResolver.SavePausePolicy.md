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
  - llm/validation
  - llm/di
---
# IArtifactResolver::SavePausePolicy
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Persist a chapter’s pause policy artifact to the chapter-level `pause-policy.json` file through the resolver abstraction.**

`SavePausePolicy` is declared on `IArtifactResolver` and implemented in `FileArtifactResolver` as chapter-scoped persistence to `pause-policy.json`. The method validates input with `ArgumentNullException.ThrowIfNull(policy)`, builds the target path from `GetChapterRoot(context.Descriptor)`, and delegates serialization/write logic to `PausePolicyStorage.Save(path, policy)`. In `ChapterDocuments` it is wired in the constructor as the save delegate for the `PausePolicy` document slot.


#### [[IArtifactResolver.SavePausePolicy]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void SavePausePolicy(ChapterContext context, PausePolicy policy)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]

