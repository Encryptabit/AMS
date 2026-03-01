---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
base_class: ~
interfaces: []
member_count: 2
dependency_count: 1
pattern: ~
tags:
  - class
---

# GraphInputState

> Class in `Ams.Core.Services.Integrations.FFmpeg`

**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Dependencies
- [[AudioBuffer]] (`buffer`)

## Properties
- `Label`: string
- `Buffer`: AudioBuffer
- `Source`: AVFilterContext*
- `SampleRate`: int
- `Channels`: int
- `Frame`: AVFrame*
- `Layout`: AVChannelLayout

## Members
- [[GraphInputState..ctor]]
- [[GraphInputState.Dispose]]

