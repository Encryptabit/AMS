---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/BookCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
---
# BookCommand::CreatePopulatePhonemes
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/BookCommand.cs`


#### [[BookCommand.CreatePopulatePhonemes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreatePopulatePhonemes()
```

**Calls ->**
- [[BookCommand.PopulatePhonemesAsync]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[Log.Error]]

**Called-by <-**
- [[BookCommand.Create]]

