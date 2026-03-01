---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/utility
---
# FfEncoder::EncodeToDynamicBuffer
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Encodes an `AudioBuffer` to WAV using the dynamic-buffer sink mode and writes the result to a stream.**

This method is a thin convenience wrapper over the shared encoder pipeline. It forwards `buffer`, `output`, and either caller options or a default `new AudioEncodeOptions()` into `Encode(...)`, selecting `EncoderSink.DynamicBuffer` as the output strategy. All validation and FFmpeg work are delegated to the underlying `Encode` method.


#### [[FfEncoder.EncodeToDynamicBuffer]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void EncodeToDynamicBuffer(AudioBuffer buffer, Stream output, AudioEncodeOptions options = null)
```

**Calls ->**
- [[FfEncoder.Encode]]

