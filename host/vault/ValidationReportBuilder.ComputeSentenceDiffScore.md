---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidationReportBuilder::ComputeSentenceDiffScore
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Compute the sortable diff-mismatch score for a sentence by delegating to shared scoring logic.**

`ComputeSentenceDiffScore` is a thin adapter that delegates sentence mismatch scoring to `ComputeDiffScore`. It passes `sentence.Diff?.Stats` as the primary signal and `sentence.Metrics.Wer` as the fallback score when diff stats are unavailable. This keeps sentence ranking logic centralized while preserving a deterministic fallback path.


#### [[ValidationReportBuilder.ComputeSentenceDiffScore]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ComputeSentenceDiffScore(SentenceView sentence)
```

**Calls ->**
- [[ValidationReportBuilder.ComputeDiffScore]]

