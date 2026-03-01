---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 18
fan_in: 1
fan_out: 1
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/data-access
  - llm/validation
---
# PipelineCommand::ComputeAudioStats
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

> [!danger] High Complexity (18)
> Cyclomatic complexity: 18. Consider refactoring into smaller methods.

## Summary
**Decode an audio file and return duration, peak, true-peak, and RMS loudness statistics used by chapter stats computation.**

`ComputeAudioStats` decodes the file with `AudioProcessor.Decode(audioFile.FullName)`, short-circuits to zeroed stats when the buffer has no samples or channels, then scans planar channel data to compute sample peak and accumulated energy. During the same pass it estimates true peak with 4x-style linear interpolation (three intermediate points between adjacent samples) and builds a per-sample mean-square array averaged across channels. It then computes overall RMS, 0.5-second window RMS min/max (falling back to overall RMS if no window values were set), derives duration from `totalSamples / sampleRate`, and returns `new AudioStats(lengthSec, samplePeak, truePeak, overallRms, minWindowRms, maxWindowRms)`.


#### [[PipelineCommand.ComputeAudioStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static PipelineCommand.AudioStats ComputeAudioStats(FileInfo audioFile)
```

**Calls ->**
- [[AudioProcessor.Decode]]

**Called-by <-**
- [[PipelineCommand.ComputeChapterStats]]

