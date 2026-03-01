---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 9
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/error-handling
---
# CompactPipelineProgressReporter::BuildView
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Builds the live terminal progress view for all chapters by converting current pipeline state into a formatted status table.**

`BuildView()` constructs a Spectre.Console `Table` with minimal grey borders, expands it, and defines `Chapter`, `Stage`, `Status`, and `Active` columns. It walks `_chapterOrder`, skips missing entries via `_chapters.TryGetValue`, escapes chapter/message text with `Markup.Escape`, formats stage text through `BuildStageMarkup`, and derives an active indicator (`[red]×[/]` for failed, blinking yellow dot for running, green dot for complete). It then sets `table.Caption` to `Total runtime` using `FormatElapsed(_stopwatch.Elapsed)` and appends a blinking indicator when the run is not finished and any chapter is still running, before returning the rendered table object.


#### [[CompactPipelineProgressReporter.BuildView]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Table BuildView()
```

**Calls ->**
- [[CompactPipelineProgressReporter.BuildStageMarkup]]
- [[CompactPipelineProgressReporter.FormatElapsed]]

**Called-by <-**
- [[CompactPipelineProgressReporter.RefreshUnsafe]]
- [[CompactPipelineProgressReporter.RunAsync]]

