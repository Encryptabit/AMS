---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptAligner::ComputeCer
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Calculates character error rate between a book word range and an ASR word range using normalized text and Levenshtein distance.**

`ComputeCer` builds normalized character streams for the book span and ASR span via the two `BuildNormalizedWordString` overloads, then computes Levenshtein edit distance (`LevenshteinMetrics.Distance`) between them. If the normalized reference is empty, it returns `0.0` only when hypothesis is also empty, otherwise `1.0`. For non-empty reference, it returns `distance / Math.Max(1.0, reference.Length)` as the CER ratio.


#### [[TranscriptAligner.ComputeCer]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ComputeCer(BookIndex book, AsrResponse asr, int bookStart, int bookEnd, int? asrStart, int? asrEnd)
```

**Calls ->**
- [[LevenshteinMetrics.Distance]]
- [[TranscriptAligner.BuildNormalizedWordString]]
- [[TranscriptAligner.BuildNormalizedWordString_2]]

**Called-by <-**
- [[TranscriptAligner.Rollup_2]]

