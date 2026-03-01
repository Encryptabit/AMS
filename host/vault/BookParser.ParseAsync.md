---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "public"
complexity: 12
fan_in: 1
fan_out: 6
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# BookParser::ParseAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`

## Summary
**Validates an input file and asynchronously parses it with the appropriate format-specific parser, normalizing unexpected failures.**

`ParseAsync` is the main asynchronous dispatch pipeline for book parsing across supported formats. It validates `filePath`, enforces file existence and support (`CanParse`), then routes by lowercase extension to format-specific parsers (`ParseDocxAsync`, `ParseTextAsync`, `ParseMarkdownAsync`, `ParseRtfAsync`, `ParsePdfAsync`) via a switch expression. Unsupported extensions raise `InvalidOperationException`; unexpected parser/runtime failures are wrapped in `BookParseException`, while argument/not-found/unsupported exceptions are intentionally passed through. The method returns a `BookParseResult` from the selected backend parser.


#### [[BookParser.ParseAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<BookParseResult> ParseAsync(string filePath, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BookParser.CanParse]]
- [[BookParser.ParseDocxAsync]]
- [[BookParser.ParseMarkdownAsync]]
- [[BookParser.ParsePdfAsync]]
- [[BookParser.ParseRtfAsync]]
- [[BookParser.ParseTextAsync]]

**Called-by <-**
- [[DocumentProcessor.ParseBookAsync]]

