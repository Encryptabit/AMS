---
namespace: "Ams.Workstation.Server.Models"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Models/ProofReportModels.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Workstation.Server.Models.SentenceReport>"
member_count: 1
dependency_count: 1
pattern: ~
tags:
  - class
---

# SentenceReport

> Record in `Ams.Workstation.Server.Models`

**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Models/ProofReportModels.cs`

**Implements**:
- IEquatable

## Dependencies
- [[Ams.Workstation.Server.Models.DiffReport_]] (`Diff`)

## Properties
- `Id`: int
- `Wer`: string
- `Cer`: string
- `Status`: string
- `BookRange`: string
- `ScriptRange`: string
- `Timing`: string
- `BookText`: string
- `ScriptText`: string
- `Excerpt`: string
- `Diff`: DiffReport?
- `StartTime`: double
- `EndTime`: double
- `ParagraphId`: int?

## Members

