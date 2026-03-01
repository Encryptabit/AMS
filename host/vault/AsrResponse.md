---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrModels.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Asr.AsrResponse>"
member_count: 3
dependency_count: 0
pattern: ~
tags:
  - class
---

# AsrResponse

> Record in `Ams.Core.Asr`

**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrModels.cs`

**Implements**:
- IEquatable

## Properties
- `ModelVersion`: string
- `Tokens`: AsrToken[]
- `Segments`: AsrSegment[]
- `HasWordTimings`: bool
- `Words`: IReadOnlyList<string>
- `WordCount`: int
- `HasWords`: bool
- `_wordCache`: IReadOnlyList<string>?

## Members
- [[AsrResponse..ctor]]
- [[AsrResponse.GetWord]]
- [[AsrResponse.BuildWords]]

