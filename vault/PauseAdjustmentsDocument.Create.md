---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# PauseAdjustmentsDocument::Create
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs`


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

