---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Application.Commands.GenerateTranscriptOptions>"
member_count: 0
dependency_count: 0
pattern: ~
tags:
  - class
---

# GenerateTranscriptOptions

> Record in `Ams.Core.Application.Commands`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs`

**Implements**:
- IEquatable

## Properties
- `DefaultServiceUrl`: string
- `Default`: GenerateTranscriptOptions
- `Engine`: AsrEngine?
- `ServiceUrl`: string
- `Model`: string?
- `ModelPath`: FileInfo?
- `Language`: string
- `Threads`: int
- `UseGpu`: bool
- `GpuDevice`: int
- `BeamSize`: int
- `BestOf`: int
- `Temperature`: double
- `EnableWordTimestamps`: bool
- `EnableFlashAttention`: bool
- `EnableDtwTimestamps`: bool

