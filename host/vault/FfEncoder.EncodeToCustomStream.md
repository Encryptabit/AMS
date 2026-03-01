---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/utility
---
# FfEncoder::EncodeToCustomStream
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Encodes an audio buffer to WAV and writes it through the custom-stream FFmpeg sink path.**

This method is a small dispatch wrapper that routes encoding to the shared `Encode` implementation with a custom streaming sink mode. It passes through `buffer` and `output`, substitutes `new AudioEncodeOptions()` when `options` is null, and sets the sink to `EncoderSink.CustomStream`. The core FFmpeg setup, validation, and write loop are all handled by `Encode`.


#### [[FfEncoder.EncodeToCustomStream]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void EncodeToCustomStream(AudioBuffer buffer, Stream output, AudioEncodeOptions options = null)
```

**Calls ->**
- [[FfEncoder.Encode]]

**Called-by <-**
- [[AudioProcessor.EncodeWav]]
- [[AudioProcessor.EncodeWavToStream]]

