---
namespace: "Ams.Core.Artifacts.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/Alignment/AnchorDocument.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Artifacts.Alignment.AnchorDocument>"
member_count: 1
dependency_count: 4
pattern: ~
tags:
  - class
---

# AnchorDocument

> Record in `Ams.Core.Artifacts.Alignment`

**Path**: `Projects/AMS/host/Ams.Core/Artifacts/Alignment/AnchorDocument.cs`

**Implements**:
- IEquatable

## Dependencies
- [[Ams.Core.Artifacts.Alignment.AnchorDocumentSection_]] (`Section`)
- [[AnchorDocumentPolicy]] (`Policy`)
- [[AnchorDocumentTokenStats]] (`Tokens`)
- [[AnchorDocumentWindow]] (`Window`)

## Properties
- `SectionDetected`: bool
- `Section`: AnchorDocumentSection?
- `Policy`: AnchorDocumentPolicy
- `Tokens`: AnchorDocumentTokenStats
- `Window`: AnchorDocumentWindow
- `Anchors`: IReadOnlyList<AnchorDocumentAnchor>
- `Windows`: IReadOnlyList<AnchorDocumentWindowSegment>?

## Members

