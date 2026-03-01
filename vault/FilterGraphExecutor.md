---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
base_class: ~
interfaces:
  - "System.IDisposable"
member_count: 15
dependency_count: 2
pattern: ~
tags:
  - class
---

# FilterGraphExecutor

> Class in `Ams.Core.Services.Integrations.FFmpeg`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

**Implements**:
- IDisposable

## Dependencies
- [[Ams.Core.Services.Integrations.FFmpeg.FfFilterGraphRunner.FilterExecutionMode]] (`mode`)
- [[Ams.Core.Services.Integrations.FFmpeg.FfFilterGraphRunner.IAudioFrameSink_]] (`frameSink`)

## Properties
- `ChunkSamples`: int
- `BufferSrcFlagKeepRef`: int
- `_inputs`: GraphInputState[]
- `_filterSpec`: string
- `_mode`: FilterExecutionMode
- `_primaryMetadata`: AudioBufferMetadata?
- `_frameSink`: IAudioFrameSink?
- `_graph`: AVFilterGraph*
- `_sink`: AVFilterContext*
- `_outputFrame`: AVFrame*
- `_accumulator`: AudioAccumulator?
- `_channels`: int
- `_sampleRate`: int

## Members
- [[FilterGraphExecutor..ctor]]
- [[FilterGraphExecutor.Process]]
- [[FilterGraphExecutor.BuildOutput]]
- [[FilterGraphExecutor.SendAllFrames]]
- [[FilterGraphExecutor.SendFrame]]
- [[FilterGraphExecutor.Drain]]
- [[FilterGraphExecutor.CreateInputs]]
- [[FilterGraphExecutor.SetupSource]]
- [[FilterGraphExecutor.SetupSink]]
- [[FilterGraphExecutor.ConfigureSinkFormat]]
- [[FilterGraphExecutor.ConfigureChannelLayouts]]
- [[FilterGraphExecutor.ConfigureIntOption]]
- [[FilterGraphExecutor.ConfigureGraph]]
- [[FilterGraphExecutor.RefreshOutputFormat]]
- [[FilterGraphExecutor.Dispose]]

