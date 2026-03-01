---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookParser::SplitOnMetadataBreaks
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`

## Summary
**Splits text into meaningful fragments around metadata break patterns while discarding empty pieces.**

`SplitOnMetadataBreaks` is an iterator helper that splits text on metadata-style newline boundaries using `MetadataBreakRegex`. It returns no results for null/empty input, then trims each split fragment and yields only non-empty candidates. The method performs no additional normalization beyond trim/filter and preserves fragment order.


#### [[BookParser.SplitOnMetadataBreaks]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<string> SplitOnMetadataBreaks(string text)
```

**Called-by <-**
- [[BookParser.SplitPdfSentences]]

