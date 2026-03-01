---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PronunciationLexiconCache::AppendVariants
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`

## Summary
**It appends cleaned pronunciation variants into a set while filtering blanks and collapsing extra whitespace.**

`AppendVariants` mutates the target `HashSet<string>` by ingesting pronunciation strings from `variants`, returning immediately when `variants` is null. It skips null/whitespace items, normalizes internal spacing by splitting on spaces with `RemoveEmptyEntries` and rejoining with single spaces, and adds only non-empty normalized results to the set. Deduplication semantics are inherited from the provided set comparer (case-insensitive in current callers).


#### [[PronunciationLexiconCache.AppendVariants]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void AppendVariants(HashSet<string> set, IEnumerable<string> variants)
```

**Called-by <-**
- [[PronunciationLexiconCache.MergeVariants]]
- [[PronunciationLexiconCache.NormalizeVariants]]

