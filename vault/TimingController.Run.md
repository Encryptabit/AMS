---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 23
fan_in: 1
fan_out: 12
tags:
  - method
  - danger/high-complexity
---
# TimingController::Run
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

> [!danger] High Complexity (23)
> Cyclomatic complexity: 23. Consider refactoring into smaller methods.


#### [[TimingController.Run]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Run()
```

**Calls ->**
- [[InteractiveState.AdjustCompressionControl]]
- [[InteractiveState.MoveCompressionControlSelection]]
- [[InteractiveState.MoveWithinTier]]
- [[InteractiveState.ScrollCompressionPreview]]
- [[InteractiveState.StepInto]]
- [[InteractiveState.StepOut]]
- [[TimingController.AdjustCurrent]]
- [[TimingController.CommitCurrentScope]]
- [[TimingController.PromptForValue]]
- [[TimingController.ToggleOptionsFocus]]
- [[TimingRenderer.Dispose]]
- [[TimingRenderer.Render]]

**Called-by <-**
- [[ValidateTimingSession.RunAsync]]

