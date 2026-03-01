---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# BookIndexer::CollectLexicalTokens
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Builds a unique normalized token set from paragraph content to drive pronunciation resolution.**

`CollectLexicalTokens` extracts a deduplicated, case-insensitive lexical vocabulary from paragraph text for pronunciation lookup. It iterates each paragraph, tokenizes by whitespace, normalizes token surface text, filters non-lexical tokens, then canonicalizes each token with `PronunciationHelper.NormalizeForLookup`. Valid normalized forms are accumulated in a `HashSet<string>(StringComparer.OrdinalIgnoreCase)`, and the set is returned as `IEnumerable<string>`. This yields unique lookup keys while preserving a simple streaming extraction pipeline.


#### [[BookIndexer.CollectLexicalTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<string> CollectLexicalTokens(List<(string Text, string Style, string Kind)> paragraphs)
```

**Calls ->**
- [[PronunciationHelper.NormalizeForLookup]]
- [[BookIndexer.ContainsLexicalContent]]
- [[BookIndexer.NormalizeTokenSurface]]
- [[BookIndexer.TokenizeByWhitespace]]

**Called-by <-**
- [[BookIndexer.CreateIndexAsync]]

