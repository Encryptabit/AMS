---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# SentenceCollector::AddPause
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Adds a sentence-local pause interval and updates duration statistics for its pause class.**

`AddPause` converts the incoming `PauseSpan` into a `PauseInterval` (`class`, `start`, `end`, `hasGapHint`) and appends it to the collector’s `_pauseIntervals` list. It then records the interval duration into per-class stats via `AddDuration(span.Class, interval.OriginalDuration)`. The method mutates internal accumulation state only.


#### [[SentenceCollector.AddPause]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void AddPause(PauseSpan span)
```

**Calls ->**
- [[SentenceCollector.AddDuration]]

**Called-by <-**
- [[PauseMapBuilder.Build]]

