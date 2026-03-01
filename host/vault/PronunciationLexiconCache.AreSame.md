---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PronunciationLexiconCache::AreSame
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`

## Summary
**It determines whether two pronunciation variant lists represent the same case-insensitive membership set.**

`AreSame` compares two variant collections as case-insensitive sets rather than by ordering. It first checks `Count` equality, then builds a `HashSet<string>` from `left` with `StringComparer.OrdinalIgnoreCase` and returns `right.All(set.Contains)`. This treats permutations and case differences as equivalent while requiring identical cardinality.


#### [[PronunciationLexiconCache.AreSame]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool AreSame(IReadOnlyList<string> left, IReadOnlyList<string> right)
```

**Called-by <-**
- [[PronunciationLexiconCache.MergeAsync]]

