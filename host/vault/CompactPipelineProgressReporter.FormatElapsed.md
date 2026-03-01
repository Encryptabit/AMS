---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# CompactPipelineProgressReporter::FormatElapsed
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Convert an elapsed `TimeSpan` into a compact display string for pipeline progress output.**

`FormatElapsed` is a private static formatter that branches on `span.TotalHours >= 1` and returns `span.ToString("hh\\:mm\\:ss")` for hour-scale durations, otherwise `span.ToString("mm\\:ss")`. It is used by `BuildView` to render the runtime caption, keeping short runs compact while adding an hour field when needed. Because it uses `hh` (not total hours), durations beyond 24 hours will wrap the hour component modulo 24.


#### [[CompactPipelineProgressReporter.FormatElapsed]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatElapsed(TimeSpan span)
```

**Called-by <-**
- [[CompactPipelineProgressReporter.BuildView]]

