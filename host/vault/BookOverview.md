---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ValidationMetricsService.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Workstation.Server.Services.BookOverview>"
member_count: 1
dependency_count: 0
pattern: ~
tags:
  - class
---

# BookOverview

> Record in `Ams.Workstation.Server.Services`

**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ValidationMetricsService.cs`

**Implements**:
- IEquatable

## Properties
- `BookName`: string
- `ChapterCount`: int
- `TotalSentences`: int
- `TotalFlaggedSentences`: int
- `AvgSentenceWer`: string
- `TotalParagraphs`: int
- `TotalFlaggedParagraphs`: int
- `AvgParagraphWer`: string
- `Chapters`: IReadOnlyList<ProofChapterInfo>

## Members

