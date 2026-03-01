---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/AudioBufferMetadata.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Artifacts.AudioBufferMetadata>"
member_count: 3
dependency_count: 0
pattern: ~
tags:
  - class
---

# AudioBufferMetadata

> Record in `Ams.Core.Artifacts`

**Path**: `Projects/AMS/host/Ams.Core/Artifacts/AudioBufferMetadata.cs`

**Implements**:
- IEquatable

## Properties
- `SourcePath`: string?
- `ContainerFormat`: string?
- `CodecName`: string?
- `SourceDurationSeconds`: double?
- `SourceStartSeconds`: double?
- `SourceSampleRate`: int
- `CurrentSampleRate`: int
- `SourceChannels`: int
- `CurrentChannels`: int
- `SourceSampleFormat`: string?
- `CurrentSampleFormat`: string?
- `SourceChannelLayout`: string?
- `CurrentChannelLayout`: string?
- `Tags`: IReadOnlyDictionary<string, string>?

## Members
- [[AudioBufferMetadata.CreateDefault]]
- [[AudioBufferMetadata.WithCurrentStream]]
- [[AudioBufferMetadata.DescribeDefaultLayout]]

