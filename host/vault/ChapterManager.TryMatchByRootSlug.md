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
# ChapterManager::TryMatchByRootSlug
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Attempts to resolve an existing chapter descriptor by comparing normalized chapter IDs to normalized root-folder slugs.**

`TryMatchByRootSlug` searches existing descriptors for a slug-based match against `normalizedRequested`. It iterates descriptors, skipping blank `RootPath` values, extracts each root folder name with `Path.GetFileName(descriptor.RootPath)`, skips empty slugs, then compares `NormalizeChapterId(slug)` to `normalizedRequested` case-insensitively. It returns the first matching descriptor or `null` when no slug matches.


#### [[ChapterManager.TryMatchByRootSlug]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ChapterDescriptor TryMatchByRootSlug(IReadOnlyList<ChapterDescriptor> descriptors, string normalizedRequested)
```

**Calls ->**
- [[ChapterManager.NormalizeChapterId]]

**Called-by <-**
- [[ChapterManager.EnsureChapterDescriptor]]

