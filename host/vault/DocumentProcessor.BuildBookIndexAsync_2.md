---
namespace: "Ams.Core.Processors.DocumentProcessor"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Indexing.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/utility
---
# DocumentProcessor::BuildBookIndexAsync
**Path**: `Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Indexing.cs`

## Summary
**Builds a `BookIndex` directly from a source file by parsing it and then running the standard index-construction pipeline.**

This overload composes parsing and indexing: it first awaits `ParseBookAsync(sourceFile, cancellationToken)`, then forwards the result to the parse-result overload `BuildBookIndexAsync(parseResult, sourceFile, options, pronunciationProvider, cancellationToken)`. It is an async convenience entry point with no extra indexing logic beyond orchestration.


#### [[DocumentProcessor.BuildBookIndexAsync_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<BookIndex> BuildBookIndexAsync(string sourceFile, BookIndexOptions options = null, IPronunciationProvider pronunciationProvider = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[DocumentProcessor.BuildBookIndexAsync]]
- [[DocumentProcessor.ParseBookAsync]]

