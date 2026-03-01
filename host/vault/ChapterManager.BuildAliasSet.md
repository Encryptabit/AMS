---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# ChapterManager::BuildAliasSet
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Builds normalized chapter aliases and resolves the best matching book section from chapter/root labels.**

`BuildAliasSet` assembles a case-insensitive alias collection for chapter matching and returns a resolved section via `out matchedSection`. It seeds aliases with `chapterId`, optionally adds the root-folder name, then resolves section candidates in priority order: direct `chapterId`, root-name fallback, then iterative alias probing (`TryResolveSectionFromAliases`). If a section is found and has a title, that title is added as an additional alias. The method returns the final alias set as an array.


#### [[ChapterManager.BuildAliasSet]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyCollection<string> BuildAliasSet(string chapterId, string chapterRoot, BookIndex bookIndex, out SectionRange matchedSection)
```

**Calls ->**
- [[ChapterManager.AddAlias]]
- [[ChapterManager.TryResolveSection]]
- [[ChapterManager.TryResolveSectionFromAliases]]

**Called-by <-**
- [[ChapterManager.CreateContext]]

