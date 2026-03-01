---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookParser::SplitPdfSentences
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`

## Summary
**Breaks sanitized PDF text into sentence/paragraph fragments using punctuation and metadata-break heuristics.**

`SplitPdfSentences` yields cleaned sentence-like fragments from PDF text using layered splitting heuristics. It short-circuits on blank input, normalizes typography and line endings, then splits by `PdfSentenceSplitRegex` (terminal punctuation boundaries or blank-line breaks). If regex splitting does not produce multiple segments, it falls back to `SplitOnMetadataBreaks(normalized)`; otherwise each segment is further passed through `SplitOnMetadataBreaks` to peel metadata-style line breaks. Only non-empty fragments are yielded.


#### [[BookParser.SplitPdfSentences]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<string> SplitPdfSentences(string text)
```

**Calls ->**
- [[TextNormalizer.NormalizeTypography]]
- [[BookParser.SplitOnMetadataBreaks]]

**Called-by <-**
- [[BookParser.ParsePdfAsync]]

