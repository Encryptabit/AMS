---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 3
tags:
  - method
---
# PipelineCommand::ResolveProcessedVariants
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


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

