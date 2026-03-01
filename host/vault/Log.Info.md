---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 1
fan_in: 6
fan_out: 0
tags:
  - method
  - llm/utility
---
# Log::Info
**Path**: `Projects/AMS/host/Ams.Core/Common/Log.cs`

## Summary
**Writes an information-level log message through the shared `Log` facade.**

`Info` is an expression-bodied wrapper that delegates directly to `logger.LogInformation(message, args)` on the class-level shared logger. The method contains no control flow, validation, or enrichment logic, so template parsing and argument binding are handled by the underlying logging framework.


#### [[Log.Info]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Info(string message, params object[] args)
```

**Called-by <-**
- [[DspCommand.CreateFilterChainInitCommand]]
- [[DspCommand.CreateFilterChainRunCommand]]
- [[DspCommand.CreateFiltersCommand]]
- [[DspCommand.ExecuteFilterChain]]
- [[TreatCommand.Create]]
- [[AsrEngineConfig.DownloadModelIfMissingAsync]]

