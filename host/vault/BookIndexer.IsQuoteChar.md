---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# BookIndexer::IsQuoteChar
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Checks whether a character is one of the supported outer-quote delimiters.**

`IsQuoteChar` is a tiny predicate that classifies a character as a quote delimiter used by token normalization. It uses a C# pattern match (`ch is '"' or '\''`) and returns `true` only for double-quote and single-quote characters. No normalization or locale-specific handling is applied.


#### [[BookIndexer.IsQuoteChar]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsQuoteChar(char ch)
```

**Called-by <-**
- [[BookIndexer.TrimOuterQuotes]]

