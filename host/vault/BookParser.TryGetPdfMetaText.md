---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# BookParser::TryGetPdfMetaText
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`

## Summary
**Safely reads and decodes a metadata tag from a PDF document, returning `null` when unavailable or on failure.**

`TryGetPdfMetaText` performs best-effort extraction of a PDF metadata field via PDFium with manual unmanaged buffer handling. It first queries required buffer length (`FPDF_GetMetaText(..., nint.Zero, 0)`), returns `null` for empty values (`<= 2` bytes), allocates native memory, re-queries the value, copies bytes to managed memory, decodes as UTF-16 (`Encoding.Unicode`) excluding the trailing null terminator, and trims whitespace. Native memory is always released in `finally`, and any exception is swallowed with a `null` fallback.


#### [[BookParser.TryGetPdfMetaText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string TryGetPdfMetaText(FpdfDocumentT document, string tag)
```

**Called-by <-**
- [[BookParser.ParsePdfAsync]]

