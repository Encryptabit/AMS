---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::ContainsLexicalContent
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Determines whether text contains meaningful lexical characters suitable for indexing.**

`ContainsLexicalContent` is a lightweight lexical predicate that checks whether input contains any letter or digit characters. It returns `false` for null/empty input, then scans characters and returns `true` on the first `char.IsLetterOrDigit(c)` match; otherwise `false`. This avoids regex overhead and treats punctuation/whitespace-only strings as non-lexical.


#### [[BookIndexer.ContainsLexicalContent]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool ContainsLexicalContent(string text)
```

**Called-by <-**
- [[BookIndexer.CollectLexicalTokens]]
- [[BookIndexer.FoldAdjacentHeadings]]
- [[BookIndexer.Process]]

