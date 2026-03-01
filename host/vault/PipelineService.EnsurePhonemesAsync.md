---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 3
tags:
  - method
  - llm/async
  - llm/utility
  - llm/validation
---
# PipelineService::EnsurePhonemesAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

## Summary
**Conditionally backfill missing word phonemes in a `BookIndex` and keep the original data when enrichment provides no improvement.**

`EnsurePhonemesAsync` computes `missingBefore` via `CountMissingPhonemes` and short-circuits when no words need phoneme enrichment. If missing entries exist, it asynchronously calls `DocumentProcessor.PopulateMissingPhonemesAsync(index, _pronunciationProvider, cancellationToken)`, then recomputes `missingAfter`. It only adopts the enriched `BookIndex` when missing phonemes were actually reduced, emitting `Log.Debug` with the added and remaining counts; otherwise it returns the original index unchanged.


#### [[PipelineService.EnsurePhonemesAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<BookIndex> EnsurePhonemesAsync(BookIndex index, CancellationToken cancellationToken)
```

**Calls ->**
- [[Log.Debug]]
- [[DocumentProcessor.PopulateMissingPhonemesAsync]]
- [[PipelineService.CountMissingPhonemes]]

**Called-by <-**
- [[PipelineService.BuildBookIndexAsync]]
- [[PipelineService.BuildBookIndexInternal]]

