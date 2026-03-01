---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PipelineService::IsStageEnabled
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

## Summary
**Determines whether a given pipeline stage should run based on normalized start/end stage bounds in `PipelineRunOptions`.**

`IsStageEnabled` computes an inclusive execution window by converting enum values to `int`, clamping `options.StartStage` to at least `PipelineStage.BookIndex`, and clamping `options.EndStage` to at most `PipelineStage.Mfa`. It then returns `true` only when the candidate `stage` value is between those normalized bounds (`value >= start && value <= end`). In `RunChapterAsync`, this acts as the stage gate before per-stage force/existence logic, ensuring non-executable enum states like `Pending` and `Complete` do not drive stage execution.


#### [[PipelineService.IsStageEnabled]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsStageEnabled(PipelineStage stage, PipelineRunOptions options)
```

**Called-by <-**
- [[PipelineService.RunChapterAsync]]

