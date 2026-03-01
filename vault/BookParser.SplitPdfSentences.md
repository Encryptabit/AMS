---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 2
tags:
  - method
---
# BookParser::SplitPdfSentences
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`


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

