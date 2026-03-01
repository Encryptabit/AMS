---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseWindowSnapshot::FromWindow
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs`

## Summary
**Builds a serializable pause-window snapshot from a runtime pause-window object.**

`FromWindow` creates a `PauseWindowSnapshot` from a `PauseWindow` after enforcing a null guard (`ArgumentNullException`). It copies `window.Min` and `window.Max` directly into the snapshot record with no additional normalization.


#### [[PauseWindowSnapshot.FromWindow]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PauseWindowSnapshot FromWindow(PauseWindow window)
```

**Called-by <-**
- [[PausePolicySnapshot.FromPolicy]]

