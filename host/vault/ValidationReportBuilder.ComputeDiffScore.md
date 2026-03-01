---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ValidationReportBuilder::ComputeDiffScore
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Produce a stable numeric mismatch score from diff counts, with fallback behavior when diff data is unavailable.**

`ComputeDiffScore` returns a normalized mismatch ratio from diff stats, or a caller-provided fallback when stats are missing. When `stats` is present, it computes `(Insertions + Deletions) / max(1, ReferenceTokens)` to avoid divide-by-zero while preserving scale for small references. This yields a simple, comparable severity score used by both sentence and paragraph ranking helpers.


#### [[ValidationReportBuilder.ComputeDiffScore]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ComputeDiffScore(HydratedDiffStats stats, double fallback)
```

**Called-by <-**
- [[ValidationReportBuilder.ComputeParagraphDiffScore]]
- [[ValidationReportBuilder.ComputeSentenceDiffScore]]

