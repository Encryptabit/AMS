---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 3
tags:
  - method
---
# BuildIndexCommand::EnsurePhonemesAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs`


#### [[BuildIndexCommand.EnsurePhonemesAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<BookIndex> EnsurePhonemesAsync(BookIndex index, IPronunciationProvider pronunciationProvider, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BuildIndexCommand.CountMissingPhonemes]]
- [[Log.Debug]]
- [[DocumentProcessor.PopulateMissingPhonemesAsync]]

**Called-by <-**
- [[BuildIndexCommand.BuildBookIndexAsync]]

