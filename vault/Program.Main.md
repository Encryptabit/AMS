---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 4
fan_in: 0
fan_out: 19
tags:
  - method
---
# Program::Main
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Program.cs`


#### [[Program.Main]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<int> Main(string[] args)
```

**Calls ->**
- [[AlignCommand.Create]]
- [[AsrCommand.Create]]
- [[BookCommand.Create]]
- [[BuildIndexCommand.Create]]
- [[DspCommand.Create]]
- [[PipelineCommand.Create]]
- [[RefineSentencesCommand.Create]]
- [[TextCommand.Create]]
- [[TreatCommand.Create]]
- [[ValidateCommand.Create]]
- [[Program.ResolveDefaultAsrUrl]]
- [[Program.StartRepl]]
- [[AsrProcessSupervisor.RegisterForShutdown]]
- [[AsrProcessSupervisor.TriggerBackgroundWarmup]]
- [[MfaProcessSupervisor.RegisterForShutdown]]
- [[MfaProcessSupervisor.TriggerBackgroundWarmup]]
- [[AsrEngineConfig.Resolve]]
- [[Log.ConfigureDefaults]]
- [[Log.Debug]]

