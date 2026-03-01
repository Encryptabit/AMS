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
# ChapterCollector::AddPause
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Registers a pause for a paragraph boundary and contributes its duration to chapter-level class aggregates.**

`AddPause` inserts a `PauseInterval` into `_pausesByParagraph` under the given `leftParagraphId`, creating the backing `List<PauseInterval>` on first use via `TryGetValue`. After storing the interval, it forwards `interval.Class` and `interval.OriginalDuration` to `AddDuration`. This keeps chapter-level pause placement and per-class duration statistics updated in one operation.


#### [[ChapterCollector.AddPause]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void AddPause(int leftParagraphId, PauseInterval interval)
```

**Calls ->**
- [[ChapterCollector.AddDuration]]

**Called-by <-**
- [[PauseMapBuilder.Build]]

