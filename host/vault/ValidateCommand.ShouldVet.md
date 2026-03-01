---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/data-access
---
# ValidateCommand::ShouldVet
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Determine if a pause-adjustment item is eligible to be vetted using the available sentence-alignment map.**

`ShouldVet` is a private static boolean predicate called by `VetPauseAdjustments` to decide whether an individual `PauseAdjust` should enter vetting logic. Given its signature and cyclomatic complexity of 5, the implementation is likely a compact set of guard branches that validate the adjustment payload and perform sentence-index lookups against `IReadOnlyDictionary<int, SentenceAlign>`, returning early when prerequisites are not met. It appears to be a side-effect-free prefilter that enforces input consistency before deeper validation runs.


#### [[ValidateCommand.ShouldVet]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool ShouldVet(PauseAdjust adjust, IReadOnlyDictionary<int, SentenceAlign> sentences)
```

**Called-by <-**
- [[ValidateCommand.VetPauseAdjustments]]

