---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseAdjustmentsDocument::Create
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs`

## Summary
**Constructs a serializable pause-adjustments document from transcript metadata, a policy snapshot, and sanitized adjustment entries.**

`Create` builds a `PauseAdjustmentsDocument` from runtime pause-adjustment inputs after validating required dependencies (`policy` and `adjustments` cannot be null). It materializes `adjustments` to a list while filtering out null entries, then snapshots the mutable policy via `PausePolicySnapshot.FromPolicy(policy)` before constructing the immutable record. `sourceTranscript` and `generatedAtUtc` are passed through as provided.


#### [[PauseAdjustmentsDocument.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PauseAdjustmentsDocument Create(string sourceTranscript, DateTime generatedAtUtc, PausePolicy policy, IEnumerable<PauseAdjust> adjustments)
```

**Calls ->**
- [[PausePolicySnapshot.FromPolicy]]

**Called-by <-**
- [[ValidateTimingSession.PersistPauseAdjustments]]

