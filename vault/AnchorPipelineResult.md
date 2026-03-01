---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPipeline.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Core.Processors.Alignment.Anchors.AnchorPipelineResult>"
member_count: 1
dependency_count: 1
pattern: ~
tags:
  - class
---

# AnchorPipelineResult

> Record in `Ams.Core.Processors.Alignment.Anchors`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPipeline.cs`

**Implements**:
- IEquatable

## Dependencies
- [[Ams.Core.Runtime.Book.SectionRange_]] (`Section`)

## Properties
- `SectionDetected`: bool
- `Section`: SectionRange?
- `Anchors`: IReadOnlyList<Anchor>
- `BookWindowFiltered`: (int bStart, int bEnd)
- `BookTokenCount`: int
- `BookFilteredCount`: int
- `AsrTokenCount`: int
- `AsrFilteredCount`: int
- `Windows`: IReadOnlyList<(int bLo, int bHi, int aLo, int aHi)>?
- `BookFilteredToOriginalWord`: IReadOnlyList<int>

## Members

