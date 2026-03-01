---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# PipelineCommand::WriteVerificationCsv
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.WriteVerificationCsv]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void WriteVerificationCsv(string path, string chapterLabel, string variantLabel, AudioVerificationResult result)
```

**Calls ->**
- [[PipelineCommand.EscapeCsv]]

**Called-by <-**
- [[PipelineCommand.RunVerify]]

