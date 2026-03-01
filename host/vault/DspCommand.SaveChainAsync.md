---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/async
  - llm/utility
---
# DspCommand::SaveChainAsync
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Write the provided `TreatmentChain` to the specified file as pretty-printed camelCase JSON, honoring cancellation.**

`SaveChainAsync` creates `JsonSerializerOptions` with `WriteIndented = true` and `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`, then guarantees the target folder exists using `Directory.CreateDirectory(chainFile.DirectoryName ?? Directory.GetCurrentDirectory())`. It opens `chainFile.FullName` with `FileMode.Create`, `FileAccess.Write`, and `FileShare.None` in an `await using` `FileStream`, and serializes the `TreatmentChain` with `JsonSerializer.SerializeAsync(..., cancellationToken)`. The method is fully async and explicitly uses `ConfigureAwait(false)` on the serialization await.


#### [[DspCommand.SaveChainAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task SaveChainAsync(FileInfo chainFile, TreatmentChain chain, CancellationToken cancellationToken)
```

**Called-by <-**
- [[DspCommand.CreateChainAddCommand]]
- [[DspCommand.CreateChainInsertCommand]]
- [[DspCommand.CreateChainPrependCommand]]
- [[DspCommand.CreateChainRemoveCommand]]

