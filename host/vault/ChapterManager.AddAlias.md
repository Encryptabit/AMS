---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# ChapterManager::AddAlias
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Adds a raw alias and its normalized chapter-ID variant into an alias set when applicable.**

`AddAlias` conditionally inserts alias values into a set with normalization-aware expansion. It ignores null/whitespace input, then adds the raw value to `aliases`; only when that insertion is new does it compute `NormalizeChapterId(value)` and add the normalized token if non-empty. This avoids repeated normalization work for duplicates while keeping both original and canonical alias forms.


#### [[ChapterManager.AddAlias]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void AddAlias(ISet<string> aliases, string value)
```

**Calls ->**
- [[ChapterManager.NormalizeChapterId]]

**Called-by <-**
- [[ChapterManager.BuildAliasSet]]
- [[ChapterManager.MergeAliases]]

