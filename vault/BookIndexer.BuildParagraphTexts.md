---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# BookIndexer::BuildParagraphTexts
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`


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

