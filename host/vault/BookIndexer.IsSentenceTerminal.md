---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::IsSentenceTerminal
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Checks whether a token should be treated as sentence-ending punctuation.**

`IsSentenceTerminal` determines whether a token ends a sentence by inspecting its final meaningful character. It returns `false` for null/empty input, then walks backward stripping common closing punctuation (`)]}'\"»”’`) before evaluating the terminal char. It returns `true` only when that char is `.`, `!`, `?`, or `…`; otherwise `false`.


#### [[BookIndexer.IsSentenceTerminal]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsSentenceTerminal(string token)
```

**Called-by <-**
- [[BookIndexer.Process]]

