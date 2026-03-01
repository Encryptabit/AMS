---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Artifacts/SentenceTiming.cs"
access_modifier: "public"
base_class: "Ams.Core.Artifacts.TimingRange"
interfaces:
  - "System.IEquatable<Ams.Core.Artifacts.SentenceTiming>"
member_count: 6
dependency_count: 1
pattern: ~
tags:
  - class
---

# SentenceTiming

> Record in `Ams.Core.Artifacts`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Artifacts/SentenceTiming.cs`

**Inherits from**: [[TimingRange]]

**Implements**:
- IEquatable

## Dependencies
- [[TimingRange]] (`range`)

## Properties
- `FragmentBacked`: bool
- `Confidence`: double?

## Members
- [[SentenceTiming..ctor_2]]
- [[SentenceTiming..ctor]]
- [[SentenceTiming.WithFragmentBacked]]
- [[SentenceTiming.WithConfidence]]
- [[SentenceTiming.WithStart]]
- [[SentenceTiming.WithEnd]]

