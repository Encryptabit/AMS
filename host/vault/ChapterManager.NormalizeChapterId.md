---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 4
fan_in: 4
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ChapterManager::NormalizeChapterId
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Converts chapter labels into a normalized alphanumeric lowercase key used for descriptor matching.**

`NormalizeChapterId` canonicalizes chapter identifiers for matching by keeping only alphanumeric characters and lowercasing them. It returns `string.Empty` for null/whitespace input, then iterates characters, appending `char.ToLowerInvariant(ch)` only when `char.IsLetterOrDigit(ch)` is true. The resulting compact token supports robust alias/root/ID comparisons across formatting variations.


#### [[ChapterManager.NormalizeChapterId]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeChapterId(string value)
```

**Called-by <-**
- [[ChapterManager.AddAlias]]
- [[ChapterManager.EnsureChapterDescriptor]]
- [[ChapterManager.FindByAlias]]
- [[ChapterManager.TryMatchByRootSlug]]

