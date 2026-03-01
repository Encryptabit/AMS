---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
base_class: ~
interfaces:
  - "Ams.Cli.Commands.PipelineCommand.IPipelineProgressReporter"
member_count: 8
dependency_count: 0
pattern: ~
tags:
  - class
---

# PipelineProgressReporter

> Class in `Ams.Cli.Commands`

**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

**Implements**:
- [[IPipelineProgressReporter]]

## Properties
- `_sync`: object
- `_tasks`: Dictionary<string, ProgressTask>

## Members
- [[PipelineProgressReporter..ctor]]
- [[PipelineProgressReporter.SetQueued]]
- [[PipelineProgressReporter.MarkRunning]]
- [[PipelineProgressReporter.ReportStage]]
- [[PipelineProgressReporter.MarkComplete]]
- [[PipelineProgressReporter.MarkFailed]]
- [[PipelineProgressReporter.Update]]
- [[PipelineProgressReporter.BuildDescription]]

