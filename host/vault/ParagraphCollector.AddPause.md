---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# ParagraphCollector::AddPause
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Registers a pause interval for a specific sentence boundary and records its duration in per-class paragraph stats.**

`AddPause` groups a `PauseInterval` under the provided `leftSentenceId` in `_pausesBySentence`, creating the per-sentence `List<PauseInterval>` on first use via `TryGetValue`. After appending the interval, it calls `AddDuration(interval.Class, interval.OriginalDuration)` to update class-level duration aggregates. This keeps sentence-level pause placement and paragraph-level pause statistics synchronized from a single insertion path.


#### [[ParagraphCollector.AddPause]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void AddPause(int leftSentenceId, PauseInterval interval)
```

**Calls ->**
- [[ParagraphCollector.AddDuration]]

**Called-by <-**
- [[PauseMapBuilder.Build]]

