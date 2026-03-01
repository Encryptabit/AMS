---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 8
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::TokenizeByWhitespace
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Lazily splits text into tokens separated by whitespace while preserving raw token content.**

`TokenizeByWhitespace` is an iterator-based tokenizer that lazily yields contiguous non-whitespace spans from an input string. It returns no tokens for null/empty input (`yield break`), then uses index scanning to skip whitespace and emit `text[start..i]` slices for each token segment. The implementation avoids allocations from `Split` and preserves original token text exactly.


#### [[BookIndexer.TokenizeByWhitespace]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<string> TokenizeByWhitespace(string text)
```

**Called-by <-**
- [[BookIndexer.CollectLexicalTokens]]
- [[BookIndexer.Process]]

