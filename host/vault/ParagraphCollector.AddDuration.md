---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ParagraphCollector::AddDuration
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Adds a validated pause duration to the paragraph-level duration bucket for a given pause class.**

`AddDuration` updates the paragraph collector’s `_durations` dictionary keyed by `PauseClass`, allocating a new `List<double>` when no bucket exists for the class. It then validates the sample (`duration >= 0d && double.IsFinite(duration)`) before appending, ignoring negative, `NaN`, or infinite inputs. This keeps duration aggregates clean for downstream stats computation.


#### [[ParagraphCollector.AddDuration]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AddDuration(PauseClass pauseClass, double duration)
```

**Called-by <-**
- [[ParagraphCollector.AddPause]]

