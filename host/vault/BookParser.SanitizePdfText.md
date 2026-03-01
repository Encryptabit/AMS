---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 12
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookParser::SanitizePdfText
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`

## Summary
**Cleans PDF text by removing invalid/control artifacts while preserving legitimate Unicode content and basic whitespace controls.**

`SanitizePdfText` filters raw PDF-extracted text by iterating characters and rebuilding a cleaned string in a `StringBuilder`. It passes through valid characters but drops Unicode noncharacters (`\uFFFE`, `\uFFFF`), invalid lone surrogates, and control characters except `\n`, `\r`, and `\t`; valid surrogate pairs are preserved using a stack-allocated 2-char span. Null/empty input is returned unchanged. This preserves readable textual content while removing common PDF extraction artifacts.


#### [[BookParser.SanitizePdfText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string SanitizePdfText(string text)
```

**Called-by <-**
- [[BookParser.ParsePdfAsync]]

