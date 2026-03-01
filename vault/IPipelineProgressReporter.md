---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
base_class: ~
interfaces: []
member_count: 5
dependency_count: 0
pattern: ~
tags:
  - interface
---

# IPipelineProgressReporter

> Interface in `Ams.Cli.Commands`

**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Members
- [[IPipelineProgressReporter.SetQueued]]
- [[IPipelineProgressReporter.MarkRunning]]
- [[IPipelineProgressReporter.ReportStage]]
- [[IPipelineProgressReporter.MarkComplete]]
- [[IPipelineProgressReporter.MarkFailed]]

## Known Implementors
- [[PipelineProgressReporter]]
- [[CompactPipelineProgressReporter]]
- [[NullProgressReporter]]

