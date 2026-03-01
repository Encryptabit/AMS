---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# ValidationReportBuilder::ExtractSourceInfo
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Create a unified source-metadata record for the validation report from transcript or hydrated input.**

`ExtractSourceInfo` selects report source metadata from `TranscriptIndex` when available, otherwise falls back to `HydratedTranscript`. It constructs a `SourceInfo` with `AudioPath`, `ScriptPath`, `BookIndexPath`, and `CreatedAtUtc` from the chosen source. The hydrated branch uses a null-forgiving access (`hydrated!`) because nullability is guaranteed by `Build`’s upfront validation.


#### [[ValidationReportBuilder.ExtractSourceInfo]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static SourceInfo ExtractSourceInfo(TranscriptIndex tx, HydratedTranscript hydrated)
```

**Called-by <-**
- [[ValidationReportBuilder.Build]]

