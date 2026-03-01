---
namespace: "Ams.Workstation.Server.Models"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Models/ProofReportModels.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Workstation.Server.Models.ChapterReport>"
member_count: 1
dependency_count: 1
pattern: ~
tags:
  - class
---

# ChapterReport

> Record in `Ams.Workstation.Server.Models`

**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Models/ProofReportModels.cs`

**Implements**:
- IEquatable

## Dependencies
- [[Ams.Workstation.Server.Models.ChapterStats]] (`Stats`)

## Properties
- `ChapterName`: string
- `AudioPath`: string
- `ScriptPath`: string
- `Created`: DateTime
- `Stats`: ChapterStats
- `Sentences`: IReadOnlyList<SentenceReport>
- `Paragraphs`: IReadOnlyList<ParagraphReport>

## Members

