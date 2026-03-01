---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 6
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptAligner::BuildNormalizedWordString
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Builds a normalized character sequence from a bounded range of book tokens for text-similarity/error metrics.**

`BuildNormalizedWordString(BookIndex, int, int)` returns a canonical concatenated alphanumeric string for a book word range. It short-circuits to `string.Empty` when `book` is null, indices are invalid (`end < start` or `start < 0`), or the word array is empty; otherwise it clamps `end` to the last token, iterates from `Math.Max(0, start)` to that safe end, and feeds each word’s `Text` into `AppendNormalized`. Normalization is delegated to `AppendNormalized`, which strips non-alphanumerics and lowercases characters.


#### [[TranscriptAligner.BuildNormalizedWordString_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildNormalizedWordString(BookIndex book, int start, int end)
```

**Calls ->**
- [[TranscriptAligner.AppendNormalized]]

**Called-by <-**
- [[TranscriptAligner.ComputeCer]]
- [[TranscriptAligner.Rollup_2]]

