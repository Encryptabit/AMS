---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
---
# PipelineCommand::ResolveVariantHydrate
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Resolve the most appropriate hydrate JSON path for a pause-adjusted variant under a chapter directory.**

ResolveVariantHydrate is a nullable filesystem resolver used by ResolveProcessedVariants to locate hydrate metadata for pause-adjusted audio in a chapter directory. It returns null immediately if chapterDir does not exist, then probes three ordered, stem-specific filenames in the chapter root and returns the first existing file. If none match, it falls back to a recursive search for *.pause-adjusted.hydrate.json and returns the first hit (or null).


#### [[PipelineCommand.ResolveVariantHydrate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveVariantHydrate(DirectoryInfo chapterDir, string stem)
```

**Called-by <-**
- [[PipelineCommand.ResolveProcessedVariants]]

