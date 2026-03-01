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
  - llm/async
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# DocumentService::PopulateMissingPhonemesAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Documents/DocumentService.cs`

## Summary
**Populates missing phoneme variants in a book index using the configured pronunciation provider.**

This method is a guarded pass-through for phoneme enrichment on an existing `BookIndex`. It first verifies that `_pronunciationProvider` was configured at service construction and throws `InvalidOperationException` with a clear message if not. When available, it delegates directly to `DocumentProcessor.PopulateMissingPhonemesAsync(index, _pronunciationProvider, cancellationToken)` and returns the resulting task.


#### [[DocumentService.PopulateMissingPhonemesAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<BookIndex> PopulateMissingPhonemesAsync(BookIndex index, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[DocumentProcessor.PopulateMissingPhonemesAsync]]

