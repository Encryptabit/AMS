---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PronunciationHelper::SplitLexemeIntoWords
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs`

## Summary
**Converts a lexeme string into cleaned component words for downstream pronunciation processing.**

`SplitLexemeIntoWords` splits a normalized lexeme into word tokens using predefined separators (`space`, `tab`, `-`). It returns `Array.Empty<string>()` for null/whitespace input; otherwise it performs `Split(..., RemoveEmptyEntries)`, trims each token, filters zero-length results, and materializes to an array. The method is a deterministic lexical utility used before pronunciation lookup.


#### [[PronunciationHelper.SplitLexemeIntoWords]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<string> SplitLexemeIntoWords(string lexeme)
```

**Called-by <-**
- [[MfaPronunciationProvider.GetPronunciationsAsync]]

