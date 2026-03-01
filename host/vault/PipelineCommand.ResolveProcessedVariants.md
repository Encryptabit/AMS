---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::ResolveProcessedVariants
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Find and normalize the best available treated and pause-adjusted chapter audio artifacts, each paired with a valid hydrate metadata path, for downstream verification.**

`ResolveProcessedVariants` assembles a `List<ProcessedVariant>` by searching for two processed WAV variants (`{stem}.treated.wav` and `{stem}.pause-adjusted.wav`) in `chapterDir` and `root`, then recursively scanning for `*.treated.wav` / `*.pause-adjusted.wav` if direct candidates are not found. It uses local helpers `TryAddVariant` and `AddVariant` to ignore blank/invalid paths, require existing files, and prevent duplicates with a case-insensitive `HashSet<string>` of full paths. For each accepted variant, it computes an effective hydrate file, preferring a resolved hydrate path (for pause-adjusted via `ResolveVariantHydrate`) only if that file exists, otherwise falling back to `referenceHydratePath`.


#### [[PipelineCommand.ResolveProcessedVariants]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<PipelineCommand.ProcessedVariant> ResolveProcessedVariants(DirectoryInfo root, DirectoryInfo chapterDir, string stem, string referenceHydratePath)
```

**Calls ->**
- [[AddVariant]]
- [[PipelineCommand.ResolveVariantHydrate]]
- [[TryAddVariant]]

**Called-by <-**
- [[PipelineCommand.RunVerify]]

