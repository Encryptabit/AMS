---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 3
tags:
  - method
---
# AsrProcessor::ShouldRetryWithoutDtw
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`


#### [[AsrProcessor.ShouldRetryWithoutDtw]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool ShouldRetryWithoutDtw(AsrOptions options, AudioBuffer buffer, AsrResponse response, out double audioDurationSec, out double transcriptEndSec, out double coverage)
```

**Calls ->**
- [[AsrProcessor.ComputeAudioDurationSeconds]]
- [[AsrProcessor.ComputeTranscriptEndSeconds]]
- [[AsrProcessor.IsDtwEffectivelyEnabled]]

**Called-by <-**
- [[AsrProcessor.TranscribeWithWhisperNetAsync]]

