---
namespace: "Ams.Core.Services.Interfaces"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Interfaces/IDocumentService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/validation
  - llm/data-access
  - llm/di
---
# IDocumentService::BuildIndexAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Interfaces/IDocumentService.cs`

## Summary
**Builds a `BookIndex` from a source file, optionally enriching missing phonemes and caching the result, with optional cache bypass.**

Implementation validates `sourceFile` via `ArgumentException.ThrowIfNullOrWhiteSpace`, then short-circuits to `_cache.GetAsync` when `forceRefresh` is false and cache is available. On cache miss, it calls `DocumentProcessor.ParseBookAsync` followed by `DocumentProcessor.BuildBookIndexAsync(parseResult, sourceFile, options, cancellationToken: cancellationToken)`. If `_pronunciationProvider` is injected, it enriches the index via `PopulateMissingPhonemesAsync`, then persists the final `BookIndex` with `_cache.SetAsync` when cache exists; all awaits use `ConfigureAwait(false)` and forward the cancellation token.


#### [[IDocumentService.BuildIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<BookIndex> BuildIndexAsync(string sourceFile, BookIndexOptions options = null, bool forceRefresh = false, CancellationToken cancellationToken = default(CancellationToken))
```

