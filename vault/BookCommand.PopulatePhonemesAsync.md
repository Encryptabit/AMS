---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/BookCommand.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 3
tags:
  - method
---
# BookCommand::PopulatePhonemesAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/BookCommand.cs`


#### [[BookCommand.PopulatePhonemesAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task PopulatePhonemesAsync(FileInfo indexFile, FileInfo outputFile, string g2pModel, CancellationToken cancellationToken)
```

**Calls ->**
- [[HasPhonemes]]
- [[Log.Debug]]
- [[DocumentProcessor.PopulateMissingPhonemesAsync]]

**Called-by <-**
- [[BookCommand.CreatePopulatePhonemes]]

