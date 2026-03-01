---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookParser.cs"
access_modifier: "private"
complexity: 14
fan_in: 2
fan_out: 0
tags:
  - method
---
# BookParser::RemoveSuppressEdges
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookParser.cs`


#### [[BookParser.RemoveSuppressEdges]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string RemoveSuppressEdges(string sentence, HashSet<string> suppressList)
```

**Called-by <-**
- [[BookParser.ParseDocxAsync]]
- [[BookParser.ParsePdfAsync]]

