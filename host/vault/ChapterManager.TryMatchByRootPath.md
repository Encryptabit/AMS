---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# ChapterManager::TryMatchByRootPath
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Finds a chapter descriptor by normalized root-path equivalence.**

`TryMatchByRootPath` attempts to find an existing descriptor whose root path matches `chapterRoot` after normalization. It returns `null` for blank `chapterRoot`, normalizes the requested path with `NormalizePath`, then linearly scans descriptors, skipping blank descriptor roots and comparing normalized values case-insensitively. The first match is returned; otherwise `null`.


#### [[ChapterManager.TryMatchByRootPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ChapterDescriptor TryMatchByRootPath(IReadOnlyList<ChapterDescriptor> descriptors, string chapterRoot)
```

**Calls ->**
- [[ChapterManager.NormalizePath]]

**Called-by <-**
- [[ChapterManager.EnsureChapterDescriptor]]

