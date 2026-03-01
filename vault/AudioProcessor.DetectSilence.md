---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 3
fan_in: 3
fan_out: 4
tags:
  - method
---
# AudioProcessor::DetectSilence
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs`


#### [[AudioProcessor.DetectSilence]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<SilenceInterval> DetectSilence(AudioBuffer buffer, SilenceDetectOptions options = null)
```

**Calls ->**
- [[SilenceLogParser.Parse]]
- [[FfFilterGraph.CaptureLogs]]
- [[FfFilterGraph.Custom]]
- [[FfFilterGraph.FromBuffer]]

**Called-by <-**
- [[AudioTreatmentService.TreatChapterCoreAsync]]
- [[SpliceBoundaryService.RefineBoundary]]
- [[AudioProcessorFilterTests.DetectSilence_FindsInitialGap]]

