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
# ValidateCommand::UpdateTranscriptTimings
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Update a transcript index’s sentence timing fields using an external sentence-ID-to-timing map.**

`UpdateTranscriptTimings` is a private static helper that applies timing data from `timeline` (`IReadOnlyDictionary<int, SentenceTiming>`) onto a `TranscriptIndex`, keyed by integer sentence IDs. Given cyclomatic complexity 1, the implementation is likely a straight-through, single-pass lookup/update operation (e.g., dictionary lookup per sentence) with no branching-heavy logic. It returns a `TranscriptIndex` containing the updated timing metadata.


#### [[ValidateCommand.UpdateTranscriptTimings]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static TranscriptIndex UpdateTranscriptTimings(TranscriptIndex transcript, IReadOnlyDictionary<int, SentenceTiming> timeline)
```

