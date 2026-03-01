---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/factory
  - llm/error-handling
---
# BookParser::ParseTextAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`

## Summary
**Parses a plain-text file into normalized book parse output with inferred title, metadata, and paragraph segmentation.**

`ParseTextAsync` reads a UTF-8 text file asynchronously and builds a `BookParseResult` with best-effort metadata and paragraph structure. It heuristically infers a title from the first non-empty line when short (`<=100`) and not sentence-terminated, records metadata (`fileSize`, `encoding`), and splits paragraphs on blank-line boundaries using `_paragraphBreakRegex`, emitting `ParsedParagraph(trimmed, null, "Body")` entries. The original full text is preserved in `Text`, author is left null, and cancellation propagates while other exceptions are wrapped in `BookParseException`.


#### [[BookParser.ParseTextAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<BookParseResult> ParseTextAsync(string filePath, CancellationToken cancellationToken)
```

**Called-by <-**
- [[BookParser.ParseAsync]]

