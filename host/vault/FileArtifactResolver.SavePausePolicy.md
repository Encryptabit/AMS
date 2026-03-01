---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# FileArtifactResolver::SavePausePolicy
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Validates and saves the provided pause policy to the chapter’s `pause-policy.json` artifact.**

`SavePausePolicy` persists a chapter-scoped pause policy by first enforcing `policy` is non-null (`ArgumentNullException.ThrowIfNull`), then building the target path as `Path.Combine(GetChapterRoot(context.Descriptor), "pause-policy.json")`. It delegates actual serialization and file write behavior to `PausePolicyStorage.Save(path, policy)`, with no fallback or merge behavior.


#### [[FileArtifactResolver.SavePausePolicy]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SavePausePolicy(ChapterContext context, PausePolicy policy)
```

**Calls ->**
- [[PausePolicyStorage.Save]]
- [[FileArtifactResolver.GetChapterRoot]]

