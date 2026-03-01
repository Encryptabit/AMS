---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# PausePolicySnapshot::ToPolicy
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs`

## Summary
**Materializes a live `PausePolicy` instance from serialized snapshot data.**

`ToPolicy` reconstructs a runtime `PausePolicy` from the snapshot by converting each stored window snapshot (`Comma`, `Sentence`, `Paragraph`) via `ToWindow()` and passing all scalar fields directly to the `PausePolicy` constructor. It performs no additional validation or transformation beyond this structural mapping.


#### [[PausePolicySnapshot.ToPolicy]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PausePolicy ToPolicy()
```

**Calls ->**
- [[PauseWindowSnapshot.ToWindow]]

**Called-by <-**
- [[PausePolicyStorage.Load]]

