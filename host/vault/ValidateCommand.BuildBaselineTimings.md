---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 7
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidateCommand::BuildBaselineTimings
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Build a baseline per-sentence timing map that prefers validated hydrated timings and backfills missing IDs from transcript timings.**

`BuildBaselineTimings` creates a `Dictionary<int, SentenceTiming>` keyed by sentence ID, first populating it from `hydrated.Sentences` only when `sentence.Timing` is present and has `Duration > 0`. It then walks `transcript.Sentences` and inserts entries only for IDs not already in the map, using `transcript` timing values as fallback. This implements precedence for hydrated timings while ensuring coverage for transcript sentences that lack valid hydrated timing.


#### [[ValidateCommand.BuildBaselineTimings]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<int, SentenceTiming> BuildBaselineTimings(TranscriptIndex transcript, HydratedTranscript hydrated)
```

