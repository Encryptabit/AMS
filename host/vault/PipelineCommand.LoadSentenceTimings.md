---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/data-access
  - llm/validation
  - llm/utility
---
# PipelineCommand::LoadSentenceTimings
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Load per-sentence timing ranges from a hydrate JSON payload into an id-indexed read-only dictionary for verification logic.**

`LoadSentenceTimings` reads the hydrate JSON file from `hydratePath` using `File.OpenRead` and `JsonDocument.Parse`, then looks for a root `sentences` array and returns an empty dictionary when that shape is missing. It iterates each sentence element, keeps only records where `TryGetInt(..., "id", ...)` and `TryReadTiming(...)` both succeed, and builds a dictionary keyed by sentence id. Timings are materialized as `SentenceTiming` (via the `AudioSentenceTiming` alias), and duplicate ids are last-write-wins because assignment uses `timings[id] = ...`.


#### [[PipelineCommand.LoadSentenceTimings]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyDictionary<int, SentenceTiming> LoadSentenceTimings(string hydratePath)
```

**Calls ->**
- [[PipelineCommand.TryGetInt]]
- [[PipelineCommand.TryReadTiming]]

**Called-by <-**
- [[PipelineCommand.RunVerify]]

