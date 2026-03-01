---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 12
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# BookIndexer::FoldAdjacentHeadings
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Merges adjacent heading paragraphs into consolidated heading records to reduce duplicate/fragmented section titles.**

`FoldAdjacentHeadings` collapses consecutive heading-like paragraphs into single combined heading entries while preserving non-heading paragraphs unchanged. It scans the input list linearly, classifies each item as heading via `ContainsLexicalContent` + `ShouldStartSection`, and when a heading run is found, merges subsequent heading candidates with `CombineHeadingTitles` until a non-heading breaks the run. The method emits one folded tuple `(combinedTitle, current.Style, current.Kind)` for each heading sequence and advances the index past consumed items. For empty input it returns the original list instance.


#### [[BookIndexer.FoldAdjacentHeadings]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<(string Text, string Style, string Kind)> FoldAdjacentHeadings(List<(string Text, string Style, string Kind)> paragraphs)
```

**Calls ->**
- [[BookIndexer.CombineHeadingTitles]]
- [[BookIndexer.ContainsLexicalContent]]
- [[BookIndexer.ShouldStartSection]]

**Called-by <-**
- [[BookIndexer.CreateIndexAsync]]

