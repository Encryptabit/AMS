---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "internal"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/validation
  - llm/utility
---
# FfEncoder::CreateStreamingSink
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Builds a stream-backed FFmpeg audio frame sink with defaulted encoding options when needed.**

This factory method creates a streaming audio frame sink implementation for FFmpeg filter graph output. It validates `output` with `ArgumentNullException.ThrowIfNull`, normalizes null options to `new AudioEncodeOptions()`, and returns a new `StreamingEncoderSink` instance typed as `FfFilterGraphRunner.IAudioFrameSink`. No encoding work is done here; it only wires sink dependencies.


#### [[FfEncoder.CreateStreamingSink]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static FfFilterGraphRunner.IAudioFrameSink CreateStreamingSink(Stream output, AudioEncodeOptions options = null)
```

**Called-by <-**
- [[FfFilterGraph.StreamToWave]]

