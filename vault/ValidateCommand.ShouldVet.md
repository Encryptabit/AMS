---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
---
# ValidateCommand::ShouldVet
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`


#### [[ValidateCommand.ShouldVet]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool ShouldVet(PauseAdjust adjust, IReadOnlyDictionary<int, SentenceAlign> sentences)
```

**Called-by <-**
- [[ValidateCommand.VetPauseAdjustments]]

