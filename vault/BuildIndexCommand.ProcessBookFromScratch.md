---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 4
tags:
  - method
---
# BuildIndexCommand::ProcessBookFromScratch
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs`


#### [[BuildIndexCommand.ProcessBookFromScratch]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<BookIndex> ProcessBookFromScratch(IBookCache cache, IPronunciationProvider pronunciationProvider, string bookFilePath, BookIndexOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[Log.Debug]]
- [[DocumentProcessor.BuildBookIndexAsync]]
- [[DocumentProcessor.ParseBookAsync]]
- [[IBookCache.SetAsync]]

**Called-by <-**
- [[BuildIndexCommand.BuildBookIndexAsync]]

