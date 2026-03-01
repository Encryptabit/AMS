---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Audio.AudioMismatch>"
member_count: 1
dependency_count: 1
pattern: ~
tags:
  - class
---

# AudioMismatch

> Record in `Ams.Core.Audio`

**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

**Implements**:
- IEquatable

## Dependencies
- [[Ams.Core.Audio.AudioMismatchType]] (`Type`)

## Properties
- `StartSec`: double
- `EndSec`: double
- `Type`: AudioMismatchType
- `RawDb`: double
- `TreatedDb`: double
- `DeltaDb`: double
- `Sentences`: IReadOnlyList<SentenceSpan>

## Members

