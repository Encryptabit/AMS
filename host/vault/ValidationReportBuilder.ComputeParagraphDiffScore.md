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
# ValidationReportBuilder::ComputeParagraphDiffScore
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Calculate a paragraph’s mismatch score for sorting by delegating to the shared diff-score routine.**

`ComputeParagraphDiffScore` is a small wrapper over `ComputeDiffScore` for paragraph-level ranking. It forwards `paragraph.Diff?.Stats` as the preferred input and uses `paragraph.Metrics.Wer` as the fallback when diff stats are absent. This mirrors sentence scoring behavior and keeps the core scoring formula centralized.


#### [[ValidationReportBuilder.ComputeParagraphDiffScore]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ComputeParagraphDiffScore(ParagraphView paragraph)
```

**Calls ->**
- [[ValidationReportBuilder.ComputeDiffScore]]

