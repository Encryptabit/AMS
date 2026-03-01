---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Audio.AudioVerificationResult>"
member_count: 1
dependency_count: 0
pattern: ~
tags:
  - class
---

# AudioVerificationResult

> Record in `Ams.Core.Audio`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

**Implements**:
- IEquatable

## Properties
- `WindowMs`: double
- `StepMs`: double
- `RawSpeechThresholdDb`: double
- `TreatedSpeechThresholdDb`: double
- `DurationSec`: double
- `RawSpeechSec`: double
- `TreatedSpeechSec`: double
- `MissingSpeechSec`: double
- `ExtraSpeechSec`: double
- `Mismatches`: IReadOnlyList<AudioMismatch>

## Members

