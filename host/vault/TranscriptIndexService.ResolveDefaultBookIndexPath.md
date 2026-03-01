---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# TranscriptIndexService::ResolveDefaultBookIndexPath
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`

## Summary
**Computes the default filesystem path to the book index JSON for transcript index metadata.**

This expression-bodied private static helper deterministically builds the book-index artifact path from `BookContext`. It concatenates `bookContext.Descriptor.RootPath` with the fixed filename `"book-index.json"` via `Path.Combine`, with no branching or I/O.


#### [[TranscriptIndexService.ResolveDefaultBookIndexPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveDefaultBookIndexPath(BookContext bookContext)
```

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]

