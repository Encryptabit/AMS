---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# BuildIndexCommand::CountMissingPhonemes
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs`


#### [[BuildIndexCommand.CountMissingPhonemes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int CountMissingPhonemes(BookIndex index)
```

**Calls ->**
- [[PronunciationHelper.NormalizeForLookup]]

**Called-by <-**
- [[BuildIndexCommand.EnsurePhonemesAsync]]

