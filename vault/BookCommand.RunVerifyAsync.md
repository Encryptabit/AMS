---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/BookCommand.cs"
access_modifier: "private"
complexity: 66
fan_in: 1
fan_out: 6
tags:
  - method
  - danger/high-complexity
---
# BookCommand::RunVerifyAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/BookCommand.cs`

> [!danger] High Complexity (66)
> Cyclomatic complexity: 66. Consider refactoring into smaller methods.


#### [[BookCommand.RunVerifyAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task RunVerifyAsync(FileInfo indexFile)
```

**Calls ->**
- [[BookCommand.EndsWithLetter]]
- [[BookCommand.IsContractionSuffix]]
- [[BookCommand.IsStandaloneApostrophe]]
- [[BookCommand.Median]]
- [[BookCommand.Sha256Hex]]
- [[Log.Debug]]

**Called-by <-**
- [[BookCommand.CreateVerify]]

