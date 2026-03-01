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
  - llm/di
  - llm/error-handling
---
# IArtifactResolver::LoadPausePolicy
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Resolve and load the effective pause policy for a chapter, preferring chapter override, then book-level policy, then a built-in default.**

`LoadPausePolicy` in `IArtifactResolver` is implemented by `FileArtifactResolver` as a layered file lookup for `pause-policy.json`. It first checks the chapter-scoped file (`Path.Combine(GetChapterRoot(context.Descriptor), "pause-policy.json")`) and loads it via `PausePolicyStorage.Load`; if absent, it checks the book root (`GetBookRoot(context.Book)`) and loads that file instead. When neither file exists, it returns the default `PausePolicyPresets.House()`, which is why `ChapterDocuments` can safely bind it during constructor initialization.


#### [[IArtifactResolver.LoadPausePolicy]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
PausePolicy LoadPausePolicy(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]

