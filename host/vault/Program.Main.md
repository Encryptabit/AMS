---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 4
fan_in: 0
fan_out: 19
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/factory
  - llm/di
  - llm/utility
---
# Program::Main
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Initialize the CLI runtime and dependencies, start the REPL, and coordinate shutdown/warmup lifecycle from the application entry point.**

Main is a low-complexity async bootstrap/orchestration method that configures defaults, builds runtime components through multiple `Create(...)` factory calls, and emits startup diagnostics via `Debug(...)`. It wires graceful termination by calling `RegisterForShutdown(...)` on key services, resolves required dependencies (including a fallback ASR endpoint via `ResolveDefaultAsrUrl(...)`), then launches the interactive loop with `StartRepl(...)`. It also kicks off background initialization with two `TriggerBackgroundWarmup(...)` invocations and returns an `int` exit code via `Task<int>`.


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

