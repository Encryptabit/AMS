---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/data-access
---
# FileArtifactResolver::GetTextGridPath
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Builds the canonical path to a chapter’s MFA-generated TextGrid file.**

`GetTextGridPath` computes the expected MFA TextGrid location for a chapter. It obtains the normalized chapter root and stem via `GetChapterRoot(context.Descriptor)` and `GetChapterStem(context.Descriptor)`, builds the alignment directory (`Path.Combine(chapterRoot, "alignment", "mfa")`), then returns `<alignmentDir>/<stem>.TextGrid`. The method performs path composition only and does not access the filesystem.


#### [[FileArtifactResolver.GetTextGridPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string GetTextGridPath(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterRoot]]
- [[FileArtifactResolver.GetChapterStem]]

**Called-by <-**
- [[FileArtifactResolver.GetTextGridFile]]
- [[FileArtifactResolver.LoadTextGrid]]

