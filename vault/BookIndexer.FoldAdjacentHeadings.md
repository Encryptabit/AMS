---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 12
fan_in: 1
fan_out: 3
tags:
  - method
---
# BookIndexer::FoldAdjacentHeadings
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`


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

