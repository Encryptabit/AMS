---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 7
tags:
  - method
---
# BookParser::ParsePdfAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`


#### [[BookParser.ParsePdfAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<BookParseResult> ParsePdfAsync(string filePath, CancellationToken cancellationToken)
```

**Calls ->**
- [[AddMeta]]
- [[BookParser.EnsurePdfiumInitialized]]
- [[BookParser.RemoveSuppressEdges]]
- [[BookParser.SanitizePdfText]]
- [[BookParser.SplitPdfSentences]]
- [[BookParser.StripLeadingPageMarkers]]
- [[BookParser.TryGetPdfMetaText]]

**Called-by <-**
- [[BookParser.ParseAsync]]

