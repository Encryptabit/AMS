---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
base_class: ~
interfaces: []
member_count: 4
dependency_count: 1
pattern: ~
tags:
  - class
---

# EditablePause

> Class in `Ams.Cli.Commands`

**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Dependencies
- [[PauseSpan]] (`span`)

## Properties
- `Span`: PauseSpan
- `LeftText`: string
- `RightText`: string
- `LeftParagraphId`: int?
- `RightParagraphId`: int?
- `AdjustedDurationSec`: double
- `BaselineDurationSec`: double
- `HasChanges`: bool
- `IsIntraSentence`: bool
- `IsCrossParagraph`: bool
- `Delta`: double
- `DurationEpsilon`: double
- `_baselineDurationSec`: double

## Members
- [[EditablePause..ctor]]
- [[EditablePause.Adjust]]
- [[EditablePause.Set]]
- [[EditablePause.Commit]]

