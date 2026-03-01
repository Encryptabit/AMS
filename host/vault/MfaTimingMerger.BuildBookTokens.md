---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# MfaTimingMerger::BuildBookTokens
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`

## Summary
**Builds a normalized alignment token list for a book index range while preserving original book word indices.**

BuildBookTokens materializes the chapter book-token stream for alignment by iterating inclusive indices `[startIdx, endIdx]` and fetching raw text via `getBookToken`. Each raw token is split/normalized through `TokenizeForAlignment(..., forTextGrid: false)`, potentially yielding multiple `BookTok` entries for one `BookIdx` (e.g., hyphen splits). When no normalized token is emitted, it inserts a placeholder `BookTok(i, "")` so index correspondence does not drift. If `endIdx < startIdx`, it returns an empty list.


#### [[MfaTimingMerger.BuildBookTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<BookTok> BuildBookTokens(Func<int, string> getBookToken, int startIdx, int endIdx)
```

**Calls ->**
- [[MfaTimingMerger.TokenizeForAlignment]]

**Called-by <-**
- [[MfaTimingMerger.MergeAndApply]]

