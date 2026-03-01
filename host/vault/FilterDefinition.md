---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Cli.Commands.DspCommand.FilterDefinition>"
member_count: 3
dependency_count: 0
pattern: ~
tags:
  - class
---

# FilterDefinition

> Record in `Ams.Cli.Commands`

**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

**Implements**:
- IEquatable

## Properties
- `Name`: string
- `ParameterType`: Type?
- `Apply`: Func<FfFilterGraph, object?, FfFilterGraph>
- `DefaultParameters`: object?

## Members
- [[FilterDefinition.Create_2]]
- [[FilterDefinition.Create]]

