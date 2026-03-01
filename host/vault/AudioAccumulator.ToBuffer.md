---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 6
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/factory
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioAccumulator::ToBuffer
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Converts accumulated float samples into a finalized `AudioBuffer` with consistent stream metadata.**

`ToBuffer` materializes the accumulated channel lists into an `AudioBuffer` and derived metadata. It first enforces that an output sample rate is known (`_sampleRateSet`), throwing `InvalidOperationException` otherwise, then computes `channelCount` from `_channels.Length` and `length` from the first channel list size. Metadata is built from `templateMetadata.WithCurrentStream(_sampleRate, channelCount, "fltp", layout)` (with layout fallback via `DescribeDefaultLayout`) or `AudioBufferMetadata.CreateDefault(...)` when no template is provided, after which it allocates `new AudioBuffer(...)` and copies each channel list into `buffer.Planar[ch]`.


#### [[AudioAccumulator.ToBuffer]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBuffer ToBuffer(AudioBufferMetadata templateMetadata = null)
```

**Calls ->**
- [[AudioBufferMetadata.CreateDefault]]
- [[AudioBufferMetadata.DescribeDefaultLayout]]
- [[AudioBufferMetadata.WithCurrentStream]]

**Called-by <-**
- [[FilterGraphExecutor.BuildOutput]]

