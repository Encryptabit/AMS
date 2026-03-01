---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/BookMetadataResetService.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Workstation.Server.Services.MetadataResetResult>"
member_count: 1
dependency_count: 0
pattern: ~
tags:
  - class
---

# MetadataResetResult

> Record in `Ams.Workstation.Server.Services`

**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/BookMetadataResetService.cs`

**Implements**:
- IEquatable

## Properties
- `Success`: bool
- `BookId`: string?
- `ReviewedEntriesCleared`: int
- `IgnoredPatternsCleared`: int
- `BookScopedFilesTouched`: int
- `ClearedCurrentChapterState`: bool
- `Message`: string

## Members

