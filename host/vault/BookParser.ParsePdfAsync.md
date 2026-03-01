---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 7
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/factory
  - llm/error-handling
---
# BookParser::ParsePdfAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`

## Summary
**Asynchronously parses PDF content via PDFium into cleaned paragraph text and metadata for `BookParseResult` output.**

`ParsePdfAsync` runs PDF parsing in `Task.Run`, ensures one-time PDFium initialization, loads the PDF with `FPDF_LoadDocument`, and throws a parse exception when PDFium reports load failure. It collects document metadata via `TryGetPdfMetaText`, builds a suppression set from metadata values, then iterates pages/chars through PDFium text APIs, sanitizes extracted text (`SanitizePdfText`), splits into sentence-like units (`SplitPdfSentences`), strips page markers, removes metadata edge echoes, flattens whitespace, and emits `ParsedParagraph` entries plus aggregate text. Native resources are explicitly closed in nested `finally` blocks (`FPDFTextClosePage`, `FPDF_ClosePage`, `FPDF_CloseDocument`) and cancellation is checked per page. Any exception is wrapped as `BookParseException` at the method boundary.


#### [[BookParser.ParsePdfAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<BookParseResult> ParsePdfAsync(string filePath, CancellationToken cancellationToken)
```

**Calls ->**
- [[AddMeta]]
- [[BookParser.EnsurePdfiumInitialized]]
- [[BookParser.RemoveSuppressEdges]]
- [[BookParser.SanitizePdfText]]
- [[BookParser.SplitPdfSentences]]
- [[BookParser.StripLeadingPageMarkers]]
- [[BookParser.TryGetPdfMetaText]]

**Called-by <-**
- [[BookParser.ParseAsync]]

