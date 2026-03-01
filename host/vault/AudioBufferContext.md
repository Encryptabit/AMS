---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferContext.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 2
dependency_count: 1
pattern: ~
tags:
  - class
---

# AudioBufferContext

> Class in `Ams.Core.Runtime.Audio`

**Path**: `Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferContext.cs`

## Dependencies
- [[AudioBufferDescriptor]] (`descriptor`)

## Properties
- `Descriptor`: AudioBufferDescriptor
- `Buffer`: AudioBuffer?
- `IsLoaded`: bool
- `_descriptor`: AudioBufferDescriptor
- `_loader`: Func<AudioBufferDescriptor, AudioBuffer?>
- `_buffer`: AudioBuffer?
- `_loaded`: bool

## Members
- [[AudioBufferContext..ctor]]
- [[AudioBufferContext.Unload]]

