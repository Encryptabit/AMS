---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/async
  - llm/utility
  - llm/di
---
# BuildIndexCommand::EnsurePhonemesAsync
**Path**: `Projects/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs`

## Summary
**Conditionally backfills missing word phonemes on a `BookIndex` and returns the updated index only when phoneme coverage actually improves.**

The method first calls `CountMissingPhonemes(index)` and immediately returns the input `BookIndex` when nothing is missing. If gaps exist, it awaits `DocumentProcessor.PopulateMissingPhonemesAsync(index, pronunciationProvider, cancellationToken)`, then compares pre/post missing counts and only accepts the enriched result when `missingAfter < missingBefore`. When enrichment improves coverage, it logs the number of backfilled and remaining words via `Log.Debug`; otherwise it preserves the original index instance.


#### [[BuildIndexCommand.EnsurePhonemesAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<BookIndex> EnsurePhonemesAsync(BookIndex index, IPronunciationProvider pronunciationProvider, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BuildIndexCommand.CountMissingPhonemes]]
- [[Log.Debug]]
- [[DocumentProcessor.PopulateMissingPhonemesAsync]]

**Called-by <-**
- [[BuildIndexCommand.BuildBookIndexAsync]]

