---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs"
access_modifier: "internal"
complexity: 10
fan_in: 1
fan_out: 9
tags:
  - method
---
# BuildIndexCommand::BuildBookIndexAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs`


#### [[BuildIndexCommand.BuildBookIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static Task BuildBookIndexAsync(FileInfo bookFile, FileInfo outputFile, bool forceRefresh, BookIndexOptions options, bool noCache)
```

**Calls ->**
- [[BuildIndexCommand.EnsurePhonemesAsync]]
- [[BuildIndexCommand.FormatDuration]]
- [[BuildIndexCommand.ProcessBookFromScratch]]
- [[Log.Debug]]
- [[DocumentProcessor.CanParseBook]]
- [[DocumentProcessor.CreateBookCache]]
- [[DocumentProcessor.GetSupportedBookExtensions]]
- [[IBookCache.GetAsync]]
- [[IBookCache.SetAsync]]

**Called-by <-**
- [[BuildIndexCommand.Create]]

