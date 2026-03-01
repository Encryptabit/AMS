---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs"
access_modifier: "public"
complexity: 3
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/validation
  - llm/utility
  - llm/error-handling
---
# PauseAdjustmentsDocument::Save
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs`

## Summary
**Persists the pause-adjustments document to disk in a normalized, deterministic JSON form.**

`Save` validates the destination path, creates the parent directory when needed, and serializes the document as JSON using the class `JsonOptions`. Before serialization it deterministically orders `Adjustments` by `StartSec`, then `LeftSentenceId`, then `RightSentenceId`, and writes a cloned record (`this with { Adjustments = ordered }`) to ensure stable on-disk output. It persists via `File.WriteAllText(path, json)`.


#### [[PauseAdjustmentsDocument.Save]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Save(string path)
```

**Called-by <-**
- [[ValidateTimingSession.PersistPauseAdjustments]]
- [[FileArtifactResolver.SavePauseAdjustments]]

