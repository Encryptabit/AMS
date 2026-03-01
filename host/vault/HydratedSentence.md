---
namespace: "Ams.Core.Artifacts.Hydrate"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/Hydrate/HydratedTranscript.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Artifacts.Hydrate.HydratedSentence>"
member_count: 1
dependency_count: 5
pattern: ~
tags:
  - class
---

# HydratedSentence

> Record in `Ams.Core.Artifacts.Hydrate`

**Path**: `Projects/AMS/host/Ams.Core/Artifacts/Hydrate/HydratedTranscript.cs`

**Implements**:
- IEquatable

## Dependencies
- [[HydratedRange]] (`BookRange`)
- [[Ams.Core.Artifacts.Hydrate.HydratedScriptRange_]] (`ScriptRange`)
- [[SentenceMetrics]] (`Metrics`)
- [[Ams.Core.Artifacts.TimingRange_]] (`Timing`)
- [[Ams.Core.Artifacts.Hydrate.HydratedDiff_]] (`Diff`)

## Properties
- `Timing`: TimingRange?
- `Id`: int
- `BookRange`: HydratedRange
- `ScriptRange`: HydratedScriptRange?
- `BookText`: string
- `ScriptText`: string
- `Metrics`: SentenceMetrics
- `Status`: string
- `Diff`: HydratedDiff?

## Members

