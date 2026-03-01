---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::TrimOuterQuotes
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Removes quote characters from both ends of a string without touching inner text.**

`TrimOuterQuotes` strips leading and trailing quote characters from a string while preserving interior content. It returns `string.Empty` for null/empty input, advances a start index while `IsQuoteChar(value[start])`, retreats an end index while `IsQuoteChar(value[end])`, then returns either `string.Empty` (all quotes) or the bounded substring. The logic only trims outer quote runs and does not alter non-edge characters.


#### [[BookIndexer.TrimOuterQuotes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string TrimOuterQuotes(string value)
```

**Calls ->**
- [[BookIndexer.IsQuoteChar]]

**Called-by <-**
- [[BookIndexer.NormalizeTokenSurface]]

