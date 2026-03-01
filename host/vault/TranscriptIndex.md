---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/TranscriptModels.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Artifacts.TranscriptIndex>"
member_count: 1
dependency_count: 0
pattern: ~
tags:
  - class
---

# TranscriptIndex

> Record in `Ams.Core.Artifacts`

**Path**: `Projects/AMS/host/Ams.Core/Artifacts/TranscriptModels.cs`

**Implements**:
- IEquatable

## Properties
- `AudioPath`: string
- `ScriptPath`: string
- `BookIndexPath`: string
- `CreatedAtUtc`: DateTime
- `NormalizationVersion`: string
- `Words`: IReadOnlyList<WordAlign>
- `Sentences`: IReadOnlyList<SentenceAlign>
- `Paragraphs`: IReadOnlyList<ParagraphAlign>

## Members

