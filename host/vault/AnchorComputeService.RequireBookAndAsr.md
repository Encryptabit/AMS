---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/AnchorComputeService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
  - llm/data-access
---
# AnchorComputeService::RequireBookAndAsr
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/AnchorComputeService.cs`

## Summary
**Validates that both book index and ASR documents are loaded on the chapter context and returns them together.**

`RequireBookAndAsr` enforces prerequisite chapter state by retrieving `context.Book.Documents.BookIndex` and `context.Documents.Asr`. It throws `InvalidOperationException` with explicit messages when either artifact is missing, otherwise returns both as a tuple `(Book, Asr)`. This centralizes precondition checks for anchor computation.


#### [[AnchorComputeService.RequireBookAndAsr]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (BookIndex Book, AsrResponse Asr) RequireBookAndAsr(ChapterContext context)
```

**Called-by <-**
- [[AnchorComputeService.ComputeAnchorsAsync]]

