---
namespace: "Ams.Core.Processors.DocumentProcessor"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Indexing.cs"
access_modifier: "public"
complexity: 1
fan_in: 12
fan_out: 1
tags:
  - method
  - danger/high-fan-in
  - llm/async
  - llm/data-access
  - llm/utility
---
# DocumentProcessor::ParseBookAsync
**Path**: `Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Indexing.cs`

> [!danger] High Fan-In (12)
> This method is called by 12 other methods. Changes here have wide impact.

## Summary
**Asynchronously parses a source document into a structured `BookParseResult` using the configured book parser implementation.**

`ParseBookAsync` is a thin async wrapper that instantiates `BookParser` and forwards the call to `parser.ParseAsync(sourceFile, cancellationToken)`. It does not add pre-validation, normalization, or post-processing; all parsing behavior and error semantics come from `BookParser`.


#### [[DocumentProcessor.ParseBookAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<BookParseResult> ParseBookAsync(string sourceFile, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BookParser.ParseAsync]]

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
- [[BookParsingTests.Parser_Text_NoNormalization]]
- [[BookParsingTests.Parser_Unsupported_Throws]]

