---
namespace: "Ams.Core.Services.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Documents/DocumentService.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "Ams.Core.Services.Interfaces.IDocumentService"
member_count: 4
dependency_count: 2
pattern: "service"
tags:
  - class
  - pattern/service
---

# DocumentService

> Class in `Ams.Core.Services.Documents`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Documents/DocumentService.cs`

**Implements**:
- [[IDocumentService]]

## Dependencies
- [[Ams.Core.Runtime.Book.IPronunciationProvider_]] (`pronunciationProvider`)
- [[Ams.Core.Runtime.Book.IBookCache_]] (`cache`)

## Properties
- `_pronunciationProvider`: IPronunciationProvider?
- `_cache`: IBookCache?

## Members
- [[DocumentService..ctor]]
- [[DocumentService.BuildIndexAsync]]
- [[DocumentService.PopulateMissingPhonemesAsync]]
- [[DocumentService.ParseAndPopulatePhonemesAsync]]

