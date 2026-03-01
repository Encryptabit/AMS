---
namespace: "Ams.Core.Processors.DocumentProcessor"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Indexing.cs"
access_modifier: "public"
complexity: 1
fan_in: 10
fan_out: 1
tags:
  - method
  - danger/high-fan-in
  - llm/async
  - llm/utility
---
# DocumentProcessor::BuildBookIndexAsync
**Path**: `Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Indexing.cs`

> [!danger] High Fan-In (10)
> This method is called by 10 other methods. Changes here have wide impact.

## Summary
**Asynchronously builds a `BookIndex` from parsed book content by forwarding inputs to a `BookIndexer` instance.**

`BuildBookIndexAsync(parseResult, sourceFile, options, pronunciationProvider, cancellationToken)` is a thin orchestration wrapper that constructs a `BookIndexer` with the optional `IPronunciationProvider` and delegates indexing to `CreateIndexAsync`. It performs no additional validation or transformation of `parseResult`/`sourceFile`; indexing behavior is fully owned by `BookIndexer`.


#### [[DocumentProcessor.BuildBookIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<BookIndex> BuildBookIndexAsync(BookParseResult parseResult, string sourceFile, BookIndexOptions options = null, IPronunciationProvider pronunciationProvider = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BookIndexer.CreateIndexAsync]]

**Called-by <-**
- [[BuildIndexCommand.ProcessBookFromScratch]]
- [[DocumentProcessor.BuildBookIndexAsync_2]]
- [[DocumentService.BuildIndexAsync]]
- [[PipelineService.BuildBookIndexInternal]]
- [[BookIndexAcceptanceTests.CacheReuse_InvalidatedOnSourceChange]]
- [[BookIndexAcceptanceTests.Canonical_RoundTrip_DeterministicBytes_WithCache]]
- [[BookIndexAcceptanceTests.NoNormalization_WordsPreserveExactText]]
- [[BookIndexAcceptanceTests.Slimness_WordsContainOnlyCanonicalFields]]
- [[BookIndexAcceptanceTests.StructureRanges_CoverAllWords_NoGaps]]
- [[BookModelsTests.BookPhonemePopulator_PopulatesPhonemes]]

