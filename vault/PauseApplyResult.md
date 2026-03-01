---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Prosody.PauseApplyResult>"
member_count: 1
dependency_count: 1
pattern: ~
tags:
  - class
---

# PauseApplyResult

> Record in `Ams.Core.Prosody`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

**Implements**:
- IEquatable

## Dependencies
- [[PauseTransformSet]] (`Transforms`)

## Properties
- `Timeline`: IReadOnlyDictionary<int, SentenceTiming>
- `Transforms`: PauseTransformSet
- `IntraSentenceGaps`: IReadOnlyList<PauseIntraGap>

## Members

