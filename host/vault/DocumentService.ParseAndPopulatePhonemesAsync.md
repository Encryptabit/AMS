---
namespace: "Ams.Core.Services.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Documents/DocumentService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/validation
  - llm/error-handling
---
# DocumentService::ParseAndPopulatePhonemesAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Documents/DocumentService.cs`

## Summary
**Parses a source document and returns a newly rebuilt, phoneme-populated book index without using cached results.**

This method provides a convenience path that forces a fresh parse/index cycle while requiring pronunciation support. It checks `_pronunciationProvider` and throws `InvalidOperationException` when absent, then delegates to `BuildIndexAsync(sourceFile, options, forceRefresh: true, cancellationToken)`. By forcing refresh, it bypasses cache reuse and ensures phoneme population runs during index construction.


#### [[DocumentService.ParseAndPopulatePhonemesAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<BookIndex> ParseAndPopulatePhonemesAsync(string sourceFile, BookIndexOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[DocumentService.BuildIndexAsync]]

