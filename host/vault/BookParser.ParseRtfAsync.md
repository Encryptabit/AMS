---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/factory
  - llm/error-handling
---
# BookParser::ParseRtfAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`

## Summary
**Asynchronously parses an RTF file using DocX first and a text-cleanup fallback when structured parsing fails.**

`ParseRtfAsync` executes RTF parsing in `Task.Run` and follows a two-stage strategy. It first attempts `DocX.Load(filePath)` extraction (paragraph text/style/kind reconstruction plus metadata), and if that inner parse fails, falls back to raw UTF-8 read with best-effort RTF control-code stripping via regex before paragraph splitting. Both paths return a `BookParseResult` with format/file-size metadata and paragraph list. Outer non-cancellation failures are wrapped as `BookParseException`.


#### [[BookParser.ParseRtfAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<BookParseResult> ParseRtfAsync(string filePath, CancellationToken cancellationToken)
```

**Called-by <-**
- [[BookParser.ParseAsync]]

