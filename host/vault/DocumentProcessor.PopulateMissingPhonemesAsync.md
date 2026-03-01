---
namespace: "Ams.Core.Processors.DocumentProcessor"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Phonemes.cs"
access_modifier: "public"
complexity: 1
fan_in: 6
fan_out: 1
tags:
  - method
  - llm/async
  - llm/utility
---
# DocumentProcessor::PopulateMissingPhonemesAsync
**Path**: `Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Phonemes.cs`

## Summary
**Asynchronously fills missing phoneme data in a `BookIndex` using the provided pronunciation provider.**

`PopulateMissingPhonemesAsync` is a direct async pass-through that delegates to `BookPhonemePopulator.PopulateMissingAsync(index, pronunciationProvider, cancellationToken)`. It introduces no additional validation, batching, or merge logic; phoneme enrichment behavior is entirely handled by `BookPhonemePopulator`.


#### [[DocumentProcessor.PopulateMissingPhonemesAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<BookIndex> PopulateMissingPhonemesAsync(BookIndex index, IPronunciationProvider pronunciationProvider, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BookPhonemePopulator.PopulateMissingAsync]]

**Called-by <-**
- [[BookCommand.PopulatePhonemesAsync]]
- [[BuildIndexCommand.EnsurePhonemesAsync]]
- [[DocumentService.BuildIndexAsync]]
- [[DocumentService.PopulateMissingPhonemesAsync]]
- [[PipelineService.EnsurePhonemesAsync]]
- [[BookModelsTests.BookPhonemePopulator_PopulatesPhonemes]]

