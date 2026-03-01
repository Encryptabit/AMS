---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/TimingOverrides.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Artifacts.TimingOverridesDocument>"
member_count: 3
dependency_count: 0
pattern: ~
tags:
  - class
---

# TimingOverridesDocument

> Record in `Ams.Core.Artifacts`

**Path**: `Projects/AMS/host/Ams.Core/Artifacts/TimingOverrides.cs`

**Implements**:
- IEquatable

## Properties
- `SourceTranscript`: string
- `GeneratedAtUtc`: DateTime
- `Sentences`: IReadOnlyList<SentenceTimingOverride>
- `JsonOptions`: JsonSerializerOptions

## Members
- [[TimingOverridesDocument.Load]]
- [[TimingOverridesDocument.Save]]

