---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# PipelineService::ValidateOptions
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

## Summary
**It validates required pipeline run inputs and stops execution immediately when mandatory option values are missing.**

`ValidateOptions` is a private static guard-clause method invoked from `RunChapterAsync` to fail fast on invalid `PipelineRunOptions`. It enforces `ChapterId` via `string.IsNullOrWhiteSpace` and requires `BookFile`, `BookIndexFile`, and `AudioFile` to be non-null. On each invalid field it throws `ArgumentException` with a specific message and `nameof(options.<Property>)` as the parameter name.


#### [[PipelineService.ValidateOptions]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void ValidateOptions(PipelineRunOptions options)
```

**Called-by <-**
- [[PipelineService.RunChapterAsync]]

