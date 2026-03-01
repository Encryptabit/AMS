---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
---
# ValidateCommand::UpdateHydratedTimings
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Applies sentence timing data from a read-only timeline dictionary onto a hydrated transcript and returns the updated transcript.**

`UpdateHydratedTimings` is a private static helper that takes an existing `HydratedTranscript` and an index-keyed `IReadOnlyDictionary<int, SentenceTiming>` timeline, then returns a `HydratedTranscript` with timing data refreshed from that lookup map. With reported complexity 1, the implementation is likely a straight-line transformation (no control-flow branching), applying timeline values by sentence index and returning the updated transcript object.


#### [[ValidateCommand.UpdateHydratedTimings]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static HydratedTranscript UpdateHydratedTimings(HydratedTranscript hydrated, IReadOnlyDictionary<int, SentenceTiming> timeline)
```

