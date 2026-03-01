---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::ToggleOptionsFocus
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Toggle which options context is focused and immediately normalize compression state for the current scope.**

`ToggleOptionsFocus()` in `ValidateTimingSession.InteractiveState` is a low-complexity state-transition method that toggles the options-focus target and then invokes `EnsureCompressionStateForCurrentScope` to keep compression-related state aligned with the active scope. With complexity 2, its implementation is effectively a simple conditional/flip plus invariant enforcement. Its reuse from `RunHeadlessAsync` and another `ToggleOptionsFocus` call path indicates focus/compression coupling is intentionally centralized in this method.


#### [[InteractiveState.ToggleOptionsFocus]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ToggleOptionsFocus()
```

**Calls ->**
- [[InteractiveState.EnsureCompressionStateForCurrentScope]]

**Called-by <-**
- [[ValidateTimingSession.RunHeadlessAsync]]
- [[TimingController.ToggleOptionsFocus]]

