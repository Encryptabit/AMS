---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 1
fan_in: 9
fan_out: 0
tags:
  - method
---
# Log::Warn
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/Log.cs`


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

