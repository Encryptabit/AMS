---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::NormalizeTokenSurface
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Normalizes raw token text by applying typography cleanup, trimming, and outer-quote removal.**

`NormalizeTokenSurface` canonicalizes token text for lexical processing while preserving a deterministic transformation pipeline. It returns `string.Empty` for null/whitespace input, otherwise applies `TextNormalizer.NormalizeTypography(token).Trim()` and then removes enclosing quote characters via `TrimOuterQuotes`. The result is a cleaned token surface used by downstream lexical/content checks.


#### [[BookIndexer.NormalizeTokenSurface]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeTokenSurface(string token)
```

**Calls ->**
- [[TextNormalizer.NormalizeTypography]]
- [[BookIndexer.TrimOuterQuotes]]

**Called-by <-**
- [[BookIndexer.CollectLexicalTokens]]
- [[BookIndexer.Process]]

