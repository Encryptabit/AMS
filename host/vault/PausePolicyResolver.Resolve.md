---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Utilities/PausePolicyResolver.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# PausePolicyResolver::Resolve
**Path**: `Projects/AMS/host/Ams.Cli/Utilities/PausePolicyResolver.cs`

## Summary
**Resolve and return the applicable pause policy plus its source path from a transcript-context-aware set of candidate files.**

`Resolve(FileInfo transcriptFile = null)` is a static resolution routine that returns a tuple of the effective `PausePolicy` and the file path it came from. The method logs diagnostics via `Debug`, derives candidate policy files through `EnumerateCandidates`, normalizes/probes each candidate with `House`, and loads a policy using `Load` once a valid source is found. Its low cyclomatic complexity suggests mostly linear candidate selection with fallback behavior when inputs are null or candidates are unavailable.


#### [[PausePolicyResolver.Resolve]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static (PausePolicy Policy, string SourcePath) Resolve(FileInfo transcriptFile = null)
```

**Calls ->**
- [[PausePolicyResolver.EnumerateCandidates]]
- [[Log.Debug]]
- [[PausePolicyPresets.House]]
- [[PausePolicyStorage.Load]]

**Called-by <-**
- [[PipelineCommand.ComputeChapterStats]]
- [[ValidateTimingSession..ctor]]

