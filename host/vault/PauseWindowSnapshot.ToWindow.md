---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# PauseWindowSnapshot::ToWindow
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs`

## Summary
**Converts a pause-window snapshot back into a runtime `PauseWindow` instance.**

`ToWindow` is a direct value mapper that instantiates `new PauseWindow(Min, Max)` from the snapshot fields. It contains no validation, clamping, or side effects.


#### [[PauseWindowSnapshot.ToWindow]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PauseWindow ToWindow()
```

**Called-by <-**
- [[PausePolicySnapshot.ToPolicy]]

