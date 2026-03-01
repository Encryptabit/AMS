---
namespace: "Ams.Core.Processors.DocumentProcessor"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Phonemes.cs"
access_modifier: "public"
complexity: 1
fan_in: 6
fan_out: 1
tags:
  - method
---
# DocumentProcessor::PopulateMissingPhonemesAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Phonemes.cs`


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

