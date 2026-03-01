---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 4
tags:
  - method
  - llm/data-access
  - llm/factory
  - llm/utility
  - llm/error-handling
---
# FileArtifactResolver::LoadPausePolicy
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Loads the effective pause policy for a chapter by preferring chapter JSON, then book JSON, then a built-in default preset.**

`LoadPausePolicy` resolves pause-policy configuration with chapter-first, then book-level fallback semantics. It builds `<chapterRoot>/pause-policy.json` and returns `PausePolicyStorage.Load` when present; otherwise it checks `<bookRoot>/pause-policy.json` and loads that if found. If neither file exists, it returns the default preset via `PausePolicyPresets.House()`. This method encodes policy precedence and defaulting in a single lookup path.


#### [[FileArtifactResolver.LoadPausePolicy]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PausePolicy LoadPausePolicy(ChapterContext context)
```

**Calls ->**
- [[PausePolicyPresets.House]]
- [[PausePolicyStorage.Load]]
- [[FileArtifactResolver.GetBookRoot]]
- [[FileArtifactResolver.GetChapterRoot]]

