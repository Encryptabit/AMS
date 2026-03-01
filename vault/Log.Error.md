---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 1
fan_in: 23
fan_out: 0
tags:
  - method
  - danger/high-fan-in
---
# Log::Error
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/Log.cs`

> [!danger] High Fan-In (23)
> This method is called by 23 other methods. Changes here have wide impact.


#### [[Log.Error]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Error(Exception exception, string message, params object[] args)
```

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[AlignCommand.CreateHydrateTx]]
- [[AlignCommand.CreateTranscriptIndex]]
- [[AsrCommand.Create]]
- [[BookCommand.CreatePopulatePhonemes]]
- [[BookCommand.CreateVerify]]
- [[BuildIndexCommand.Create]]
- [[DspCommand.CreateFilterChainInitCommand]]
- [[DspCommand.CreateFilterChainRunCommand]]
- [[DspCommand.CreateInitCommand]]
- [[DspCommand.CreateListParamsCommand]]
- [[DspCommand.CreateRunCommand]]
- [[DspCommand.CreateTestAllCommand]]
- [[PipelineCommand.CreatePrepRenameCommand]]
- [[PipelineCommand.CreateRun]]
- [[PipelineCommand.CreateStatsCommand]]
- [[PipelineCommand.CreateVerifyCommand]]
- [[PipelineCommand.RunVerify]]
- [[RefineSentencesCommand.Create]]
- [[TreatCommand.Create]]
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateCommand.CreateTimingCommand]]
- [[ValidateCommand.CreateTimingInitCommand]]

