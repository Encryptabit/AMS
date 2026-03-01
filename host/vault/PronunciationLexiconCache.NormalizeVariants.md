---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# PronunciationLexiconCache::NormalizeVariants
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`

## Summary
**It converts raw pronunciation variants into a cleaned, case-insensitive, duplicate-free array.**

`NormalizeVariants` canonicalizes a pronunciation variant sequence by inserting items into a case-insensitive `HashSet<string>` through `AppendVariants`, which handles filtering/cleanup, then returning the set as an array. The output is deduplicated and normalized for consistent cache storage/comparison.


#### [[PronunciationLexiconCache.NormalizeVariants]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string[] NormalizeVariants(IEnumerable<string> variants)
```

**Calls ->**
- [[PronunciationLexiconCache.AppendVariants]]

**Called-by <-**
- [[PronunciationLexiconCache.MergeAsync]]
- [[PronunciationLexiconCache.NormalizeEntries]]

