---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 5
fan_in: 5
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfEncoder::DrainEncoder
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Pulls pending encoded audio packets from the codec and commits them to the output format context.**

This method drains all currently available encoded packets from `AVCodecContext` and writes them to the muxer stream. It loops allocating a packet, calls `avcodec_receive_packet`, stops on `EAGAIN`/`EOF`, and validates other return codes via `ThrowIfError`. For each packet it sets `stream_index`, rescales timestamps from codec to stream time base (`av_packet_rescale_ts`), writes with `av_write_frame`, and frees the packet.


#### [[FfEncoder.DrainEncoder]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void DrainEncoder(AVCodecContext* cc, AVStream* stream, AVFormatContext* fmt)
```

**Calls ->**
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfEncoder.Encode]]
- [[FfEncoder.EncodeBuffer]]
- [[FfEncoder.FlushResampler]]
- [[StreamingEncoderSink.Complete]]
- [[StreamingEncoderSink.Consume]]

