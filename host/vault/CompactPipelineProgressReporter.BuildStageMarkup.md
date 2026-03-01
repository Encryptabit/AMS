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
  - llm/error-handling
---
# CompactPipelineProgressReporter::BuildStageMarkup
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Convert a `PipelineStage` value into display-ready, safe markup for the compact pipeline progress view.**

`BuildStageMarkup` performs a `StageStyles.TryGetValue(stage, out var style)` lookup against the static `Dictionary<PipelineStage, (string Label, string Color)>`. When a style exists, it emits Spectre.Console markup in the form `[bold {style.Color}]{style.Label}[/]`; when missing, it falls back to `Markup.Escape(stage.ToString())` so unknown enum values are rendered safely as literal text.


#### [[CompactPipelineProgressReporter.BuildStageMarkup]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildStageMarkup(PipelineStage stage)
```

**Called-by <-**
- [[CompactPipelineProgressReporter.BuildView]]

