---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# PronunciationLexiconCache::MergeVariants
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`

## Summary
**It merges two pronunciation variant collections into one case-insensitive, duplicate-free array.**

`MergeVariants` combines `current` and `incoming` pronunciation sequences into a single deduplicated set using a case-insensitive `HashSet<string>`. It delegates token ingestion/cleanup to `AppendVariants` for both sources, then materializes the merged set as an array via `ToArray()`.


#### [[PronunciationLexiconCache.MergeVariants]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string[] MergeVariants(IEnumerable<string> current, IEnumerable<string> incoming)
```

**Calls ->**
- [[PronunciationLexiconCache.AppendVariants]]

**Called-by <-**
- [[PronunciationLexiconCache.MergeAsync]]

