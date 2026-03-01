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
  - llm/di
  - llm/validation
  - llm/error-handling
---
# IDocumentService::ParseAndPopulatePhonemesAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Interfaces/IDocumentService.cs`

## Summary
**Build a fresh `BookIndex` from a source file and ensure missing phonemes are populated via an injected pronunciation provider.**

`ParseAndPopulatePhonemesAsync` in `DocumentService` is a thin wrapper that enforces phoneme-capable execution by requiring `_pronunciationProvider` and throwing `InvalidOperationException` if it is missing. It then delegates directly to `BuildIndexAsync(sourceFile, options, forceRefresh: true, cancellationToken)` without `await`, returning the underlying `Task<BookIndex>`. Because `forceRefresh` is hardcoded to `true`, it bypasses cache reads in `BuildIndexAsync` while still running the normal parse/index pipeline and phoneme population path.


#### [[IDocumentService.ParseAndPopulatePhonemesAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<BookIndex> ParseAndPopulatePhonemesAsync(string sourceFile, BookIndexOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

