---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Cli.Commands.PipelineCommand.ChapterStats>"
member_count: 1
dependency_count: 2
pattern: ~
tags:
  - class
---

# ChapterStats

> Record in `Ams.Cli.Commands`

**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

**Implements**:
- IEquatable

## Dependencies
- [[AudioStats]] (`Audio`)
- [[Ams.Core.Prosody.PauseStatsSet_]] (`Prosody`)

## Properties
- `Chapter`: string
- `Audio`: AudioStats
- `Prosody`: PauseStatsSet?

## Members

