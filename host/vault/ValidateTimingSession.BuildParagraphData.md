---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/data-access
---
# ValidateTimingSession::BuildParagraphData
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Construct paragraph-level indexing from a book so timing validation can efficiently resolve sentence-to-paragraph and paragraph-to-sentence relationships.**

BuildParagraphData is a private static preprocessing helper that derives paragraph metadata from a `BookIndex` by first calling `ExtractBookText(book)`, then materializing three coordinated structures: an ordered `ParagraphInfo` list, a sentence-index-to-paragraph-index lookup, and a paragraph-index-to-sentence-indexes lookup. The tuple design enables constant-time navigation in both directions between sentence and paragraph boundaries for downstream validation in `LoadSessionContextAsync`. Its moderate complexity (6) is consistent with index bookkeeping plus paragraph-boundary condition handling during traversal.


#### [[ValidateTimingSession.BuildParagraphData]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (IReadOnlyList<ValidateTimingSession.ParagraphInfo> Paragraphs, IReadOnlyDictionary<int, int> SentenceToParagraph, IReadOnlyDictionary<int, IReadOnlyList<int>> ParagraphSentences) BuildParagraphData(BookIndex book)
```

**Calls ->**
- [[ValidateTimingSession.ExtractBookText]]

**Called-by <-**
- [[ValidateTimingSession.LoadSessionContextAsync]]

