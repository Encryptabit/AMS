---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 6
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/async
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# DspCommand::LoadChainAsync
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Load a `TreatmentChain` from disk with optional create-on-missing behavior and empty-chain fallback semantics.**

This async helper reads a chain JSON from `chainFile` using `OpenRead()` and `JsonSerializer.DeserializeAsync<TreatmentChain>` with `PropertyNameCaseInsensitive = true` and `ReadCommentHandling = Skip`. If the file is missing, it either throws `FileNotFoundException` or returns a new `TreatmentChain` based on `createIfMissing`. If deserialization returns `null`, it also returns a new empty `TreatmentChain`, and it threads the provided `CancellationToken` into deserialization.


#### [[DspCommand.LoadChainAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<TreatmentChain> LoadChainAsync(FileInfo chainFile, CancellationToken cancellationToken, bool createIfMissing = false)
```

**Called-by <-**
- [[DspCommand.CreateChainAddCommand]]
- [[DspCommand.CreateChainInsertCommand]]
- [[DspCommand.CreateChainListCommand]]
- [[DspCommand.CreateChainPrependCommand]]
- [[DspCommand.CreateChainRemoveCommand]]
- [[DspCommand.CreateRunCommand]]

