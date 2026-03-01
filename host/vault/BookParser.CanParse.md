---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# BookParser::CanParse
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`

## Summary
**Determines whether a file path points to a format supported by the book parser.**

`CanParse` performs a defensive capability check for an input path against `BookParser`’s supported extension set (`.docx`, `.txt`, `.md`, `.rtf`, `.pdf`). It returns `false` for null/whitespace input, then attempts to extract the extension via `Path.GetExtension(filePath)` and verifies membership in `_supportedExtensions`. Any exception during extension extraction is swallowed and treated as non-parseable (`false`).


#### [[BookParser.CanParse]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool CanParse(string filePath)
```

**Called-by <-**
- [[DocumentProcessor.CanParseBook]]
- [[BookParser.ParseAsync]]

