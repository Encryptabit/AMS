---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
base_class: ~
interfaces: []
member_count: 6
dependency_count: 2
pattern: "controller"
tags:
  - class
  - pattern/controller
---

# TimingController

> Class in `Ams.Cli.Commands`

**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Dependencies
- [[InteractiveState]] (`state`)
- [[TimingRenderer]] (`renderer`)

## Properties
- `_state`: InteractiveState
- `_renderer`: TimingRenderer
- `_onCommit`: Action<CommitResult>

## Members
- [[TimingController..ctor]]
- [[TimingController.Run]]
- [[TimingController.AdjustCurrent]]
- [[TimingController.ToggleOptionsFocus]]
- [[TimingController.CommitCurrentScope]]
- [[TimingController.PromptForValue]]

