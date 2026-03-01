---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Audio.FrameBreathDetectorOptions>"
member_count: 0
dependency_count: 0
pattern: ~
tags:
  - class
---

# FrameBreathDetectorOptions

> Record in `Ams.Core.Audio`

**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

**Implements**:
- IEquatable

## Properties
- `SampleRate`: int
- `FrameMs`: int
- `HopMs`: int
- `FftSize`: int
- `PreEmphasis`: double
- `HiSplitHz`: double
- `AbsFloorDb`: double
- `AmpMarginDb`: double
- `WFlat`: double
- `WHfRatio`: double
- `WZcr`: double
- `WInvNacf`: double
- `WSlope`: double
- `ScoreCenter`: double
- `ScoreHigh`: double
- `ScoreLow`: double
- `MinRunMs`: int
- `MergeGapMs`: int
- `GuardLeftMs`: int
- `GuardRightMs`: int
- `FricativeGuardMs`: int
- `Aggressiveness`: double
- `ApplyEnergyGate`: bool

