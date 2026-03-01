---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs"
access_modifier: "public"
complexity: 4
fan_in: 7
fan_out: 0
tags:
  - method
---
# CommandInputResolver::RequireAudio
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs`


#### [[CommandInputResolver.RequireAudio]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static FileInfo RequireAudio(FileInfo provided)
```

**Called-by <-**
- [[AlignCommand.CreateTranscriptIndex]]
- [[AsrCommand.Create]]
- [[DspCommand.CreateFilterChainRunCommand]]
- [[DspCommand.CreateRunCommand]]
- [[DspCommand.CreateTestAllCommand]]
- [[PipelineCommand.CreateRun]]
- [[RefineSentencesCommand.Create]]

