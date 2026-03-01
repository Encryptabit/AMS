---
namespace: "Ams.Core.Processors.DocumentProcessor"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Indexing.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
---
# DocumentProcessor::CanParseBook
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Indexing.cs`


#### [[DocumentProcessor.CanParseBook]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static bool CanParseBook(string sourceFile)
```

**Calls ->**
- [[BookParser.CanParse]]

**Called-by <-**
- [[BuildIndexCommand.BuildBookIndexAsync]]
- [[BookParsingTests.BookParser_CanParse_Extensions]]

