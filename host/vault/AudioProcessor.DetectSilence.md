---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 3
fan_in: 3
fan_out: 4
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# AudioProcessor::DetectSilence
**Path**: `Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs`

## Summary
**Detects silence regions in an audio buffer using FFmpeg’s `silencedetect` filter and parses the logs into typed intervals.**

`DetectSilence` runs FFmpeg `silencedetect` over an in-memory `AudioBuffer` by validating `buffer`, defaulting options when null, and composing a filter string with configured noise threshold and minimum duration. It builds a filter graph (`FfFilterGraph.FromBuffer(buffer)`), applies the custom filter, captures diagnostic logs, and converts those logs into structured intervals via `SilenceLogParser.Parse`. The method returns parsed `SilenceInterval` results without additional post-filtering.


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

