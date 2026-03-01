---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 3
tags:
  - method
---
# BookParser::ParseDocxAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`


#### [[BookParser.ParseDocxAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<BookParseResult> ParseDocxAsync(string filePath, CancellationToken cancellationToken)
```

**Calls ->**
- [[AddSuppress]]
- [[BookParser.RemoveSuppressEdges]]
- [[BookParser.ShouldIgnoreSuppressEntry]]

**Called-by <-**
- [[BookParser.ParseAsync]]

