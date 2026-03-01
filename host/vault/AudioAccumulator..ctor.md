---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
---
# AudioAccumulator::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Initializes channel-wise accumulation buffers and initial sample-rate tracking for filtered audio collection.**

The `AudioAccumulator` constructor preallocates per-channel sample storage and seeds output sample-rate state. It creates a `List<float>[]` sized to `channelCount`, initializes each channel list with capacity `8192`, stores the provided `sampleRate`, and sets `_sampleRateSet` to `sampleRate > 0`. It performs no explicit argument validation, so invalid `channelCount` behavior is left to array allocation/runtime checks.


#### [[AudioAccumulator..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioAccumulator(int channelCount, int sampleRate)
```

