---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Artifacts/TranscriptModels.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Artifacts.SentenceAlign>"
member_count: 2
dependency_count: 4
pattern: ~
tags:
  - class
---

# SentenceAlign

> Record in `Ams.Core.Artifacts`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Artifacts/TranscriptModels.cs`

**Implements**:
- IEquatable

## Dependencies
- [[IntRange]] (`BookRange`)
- [[Ams.Core.Artifacts.ScriptRange_]] (`ScriptRange`)
- [[TimingRange]] (`Timing`)
- [[SentenceMetrics]] (`Metrics`)

## Properties
- `Id`: int
- `BookRange`: IntRange
- `ScriptRange`: ScriptRange?
- `Timing`: TimingRange
- `Metrics`: SentenceMetrics
- `Status`: string

## Members
- [[SentenceAlign..ctor]]

