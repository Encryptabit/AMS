---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# PausePolicySnapshot::FromPolicy
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs`

## Summary
**Converts a runtime pause policy into a serializable snapshot record capturing window bounds and tuning parameters.**

`FromPolicy` creates an immutable `PausePolicySnapshot` from a live `PausePolicy` after a null guard (`ArgumentNullException`). It maps each pause window (`Comma`, `Sentence`, `Paragraph`) through `PauseWindowSnapshot.FromWindow` and copies scalar tuning parameters (`HeadOfChapter`, `PostChapterRead`, `Tail`, `KneeWidth`, `RatioInside`, `RatioOutside`, `PreserveTopQuantile`) verbatim into the snapshot record.


#### [[PausePolicySnapshot.FromPolicy]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PausePolicySnapshot FromPolicy(PausePolicy policy)
```

**Calls ->**
- [[PauseWindowSnapshot.FromWindow]]

**Called-by <-**
- [[PauseAdjustmentsDocument.Create]]
- [[PausePolicyStorage.Save]]

