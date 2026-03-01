---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
base_class: ~
interfaces:
  - "Ams.Core.Services.Integrations.FFmpeg.FfFilterGraphRunner.IAudioFrameSink"
member_count: 5
dependency_count: 1
pattern: ~
tags:
  - class
---

# StreamingEncoderSink

> Class in `Ams.Core.Services.Integrations.FFmpeg`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

**Implements**:
- [[IAudioFrameSink]]

## Dependencies
- [[AudioEncodeOptions]] (`options`)

## Properties
- `_output`: Stream
- `_options`: AudioEncodeOptions
- `_formatContext`: AVFormatContext*
- `_codecContext`: AVCodecContext*
- `_stream`: AVStream*
- `_customIo`: AVIOContext*
- `_resampler`: SwrContext*
- `_frame`: AVFrame*
- `_streamHandle`: GCHandle
- `_writeCallback`: avio_alloc_context_write_packet?
- `_inputSampleRate`: int
- `_inputChannels`: int
- `_targetSampleRate`: int
- `_pts`: long
- `_initialized`: bool
- `_completed`: bool

## Members
- [[StreamingEncoderSink..ctor]]
- [[StreamingEncoderSink.Initialize]]
- [[StreamingEncoderSink.Consume]]
- [[StreamingEncoderSink.Complete]]
- [[StreamingEncoderSink.Dispose]]

