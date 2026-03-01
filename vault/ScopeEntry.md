---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Cli.Commands.ValidateTimingSession.ScopeEntry>"
member_count: 1
dependency_count: 3
pattern: ~
tags:
  - class
---

# ScopeEntry

> Record in `Ams.Cli.Commands`

**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

**Implements**:
- IEquatable

## Dependencies
- [[Ams.Cli.Commands.ValidateTimingSession.ScopeEntryKind]] (`Kind`)
- [[Ams.Core.Prosody.PauseStatsSet_]] (`Stats`)
- [[Ams.Cli.Commands.ValidateTimingSession.EditablePause_]] (`Pause`)

## Properties
- `Kind`: ScopeEntryKind
- `Depth`: int
- `Label`: string
- `Stats`: PauseStatsSet?
- `ParagraphId`: int?
- `SentenceId`: int?
- `Pause`: EditablePause?
- `Start`: double?
- `End`: double?
- `Empty`: ScopeEntry

## Members

