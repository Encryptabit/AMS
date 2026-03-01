---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# TranscriptIndexService::RequireBookAndAsr
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`

## Summary
**Ensures both BookIndex and ASR documents are loaded in the chapter context before transcript-index computation proceeds.**

This private static guard method retrieves required inputs from `ChapterContext` and fails fast if either is missing. It reads `context.Book.Documents.BookIndex` and `context.Documents.Asr`, throwing `InvalidOperationException` with explicit messages when either document is null. On success it returns a typed tuple `(BookIndex Book, AsrResponse Asr)` for downstream alignment processing.


#### [[TranscriptIndexService.RequireBookAndAsr]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (BookIndex Book, AsrResponse Asr) RequireBookAndAsr(ChapterContext context)
```

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]

