---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs"
access_modifier: "public"
complexity: 5
fan_in: 11
fan_out: 1
tags:
  - method
  - danger/high-fan-in
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# CommandInputResolver::ResolveBookIndex
**Path**: `Projects/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs`

> [!danger] High Fan-In (11)
> This method is called by 11 other methods. Changes here have wide impact.

## Summary
**Resolve and validate a book index file argument for CLI command construction, requiring the file to exist unless explicitly disabled.**

This static helper in `Ams.Cli.Utilities.CommandInputResolver` normalizes a caller-provided `FileInfo` into the book-index path consumed by multiple `Create*` command builders. Its default `mustExist = true` introduces a validation path that checks filesystem presence before returning, with complexity 5 indicating a few guard/resolution branches. The recorded self-call to `ResolveBookIndex` implies layered resolution via recursion or an overload.


#### [[CommandInputResolver.ResolveBookIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static FileInfo ResolveBookIndex(FileInfo provided, bool mustExist = true)
```

**Calls ->**
- [[ReplState.ResolveBookIndex]]

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[AlignCommand.CreateHydrateTx]]
- [[AlignCommand.CreateTranscriptIndex]]
- [[AsrCommand.Create]]
- [[BookCommand.CreatePopulatePhonemes]]
- [[BookCommand.CreateVerify]]
- [[BuildIndexCommand.Create]]
- [[PipelineCommand.CreateRun]]
- [[TreatCommand.Create]]
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateCommand.CreateTimingCommand]]

