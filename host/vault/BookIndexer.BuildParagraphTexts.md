---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/factory
  - llm/utility
  - llm/validation
---
# BookIndexer::BuildParagraphTexts
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Builds a normalized paragraph list with style/kind metadata from parsed book input, with a text-splitting fallback path.**

`BuildParagraphTexts` materializes normalized paragraph triples `(Text, Style, Kind)` from a `BookParseResult`, preferring structured `parseResult.Paragraphs` when available. In that path it maps each paragraph through `NormalizeParagraphText`, defaulting missing metadata to `"Unknown"` style and `"Body"` kind. If structured paragraphs are absent, it falls back to splitting `parseResult.Text` on blank-line boundaries via `_blankLineSplit`, drops empty segments, trims trailing newlines, normalizes text, and assigns default metadata. The method returns a concrete `List<(string Text, string Style, string Kind)>` used by downstream indexing.


#### [[BookIndexer.BuildParagraphTexts]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<(string Text, string Style, string Kind)> BuildParagraphTexts(BookParseResult parseResult)
```

**Calls ->**
- [[BookIndexer.NormalizeParagraphText]]

**Called-by <-**
- [[BookIndexer.CreateIndexAsync]]

