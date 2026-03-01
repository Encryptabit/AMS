---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "public"
complexity: 12
fan_in: 1
fan_out: 6
tags:
  - method
---
# BookParser::ParseAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`


#### [[BookParser.ParseAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<BookParseResult> ParseAsync(string filePath, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BookParser.CanParse]]
- [[BookParser.ParseDocxAsync]]
- [[BookParser.ParseMarkdownAsync]]
- [[BookParser.ParsePdfAsync]]
- [[BookParser.ParseRtfAsync]]
- [[BookParser.ParseTextAsync]]

**Called-by <-**
- [[DocumentProcessor.ParseBookAsync]]

