---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineProgressReporter::BuildDescription
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Construct a consistently formatted, style-aware progress description string for a chapter and pipeline stage.**

`BuildDescription` formats a pipeline progress line by looking up `stage` in `StageStyles` via `TryGetValue`, with a fallback style of `("", "grey")` for unmapped stages. It conditionally builds a Spectre.Console markup label (`[bold {color}]{label}[/]`) only when the style label is non-whitespace, and similarly suppresses empty/whitespace `message` content. It then combines label and detail, prefixes a left-aligned 20-character `chapterId` (`{chapterId,-20}`), and calls `TrimEnd()` to prevent trailing spaces.


#### [[PipelineProgressReporter.BuildDescription]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildDescription(string chapterId, PipelineStage stage, string message)
```

**Called-by <-**
- [[PipelineProgressReporter..ctor]]
- [[PipelineProgressReporter.Update]]

