---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 1
fan_in: 9
fan_out: 0
tags:
  - method
  - llm/utility
---
# Log::Warn
**Path**: `Projects/AMS/host/Ams.Core/Common/Log.cs`

## Summary
**Logs a warning-level message through the central `Log` facade with optional template arguments.**

`Warn` is an expression-bodied forwarding method that invokes `logger.LogWarning(message, args)` on the static shared logger instance. It has no branching, validation, or custom formatting behavior, leaving message-template expansion and argument handling to `Microsoft.Extensions.Logging`.


#### [[Log.Warn]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Warn(string message, params object[] args)
```

**Called-by <-**
- [[DspCommand.PrintAstatsLogs]]
- [[TreatCommand.Create]]
- [[MfaWorkflow.RunChapterAsync]]
- [[AsrProcessor.CreateFactoryOptions]]
- [[AsrProcessor.TranscribeWithWhisperNetAsync]]
- [[AudioBufferManager.DefaultLoader]]
- [[BookAudio.LoadRoomtone]]
- [[PickupMatchingService.PairSegmentsToTargets]]
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

