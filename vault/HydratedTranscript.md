---
namespace: "Ams.Core.Artifacts.Hydrate"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Artifacts/Hydrate/HydratedTranscript.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Artifacts.Hydrate.HydratedTranscript>"
member_count: 1
dependency_count: 0
pattern: ~
tags:
  - class
---

# HydratedTranscript

> Record in `Ams.Core.Artifacts.Hydrate`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Artifacts/Hydrate/HydratedTranscript.cs`

**Implements**:
- IEquatable

## Properties
- `AudioPath`: string
- `ScriptPath`: string
- `BookIndexPath`: string
- `CreatedAtUtc`: DateTime
- `NormalizationVersion`: string?
- `Words`: IReadOnlyList<HydratedWord>
- `Sentences`: IReadOnlyList<HydratedSentence>
- `Paragraphs`: IReadOnlyList<HydratedParagraph>

## Members

