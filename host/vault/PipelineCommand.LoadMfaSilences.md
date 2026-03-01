---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# PipelineCommand::LoadMfaSilences
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Extract silence time ranges from an MFA TextGrid file so chapter statistics can be computed with silence-aware timing.**

LoadMfaSilences(FileInfo textGridFile) is a private static helper that parses MFA TextGrid word intervals via ParseWordIntervals, filters intervals whose labels satisfy IsSilenceLabel, and returns the resulting (Start, End) tuples as an IReadOnlyList. The method includes debug-level instrumentation through Debug to trace loading/filtering behavior. It is a low-complexity preprocessing step consumed by ComputeChapterStats to supply silence boundaries.


#### [[PipelineCommand.LoadMfaSilences]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<(double Start, double End)> LoadMfaSilences(FileInfo textGridFile)
```

**Calls ->**
- [[PipelineCommand.IsSilenceLabel]]
- [[Log.Debug]]
- [[TextGridParser.ParseWordIntervals]]

**Called-by <-**
- [[PipelineCommand.ComputeChapterStats]]

