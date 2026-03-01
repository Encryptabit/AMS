---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# PauseAdjustmentsDocument::Load
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs`

## Summary
**Loads a pause-adjustments JSON file into a `PauseAdjustmentsDocument` with explicit validation and consistent failure wrapping.**

`Load` performs strict input/file guards (`string.IsNullOrWhiteSpace` and `File.Exists`) and throws `ArgumentException`/`FileNotFoundException` on invalid path states. It reads JSON from disk with `File.ReadAllText`, deserializes `PauseAdjustmentsDocument` via `JsonSerializer.Deserialize(..., JsonOptions)`, and treats a null deserialize result as a hard failure. Any non-`FileNotFoundException` during read/deserialize is wrapped into an `InvalidOperationException` that includes the source path and original exception as `InnerException`.


#### [[PauseAdjustmentsDocument.Load]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PauseAdjustmentsDocument Load(string path)
```

**Called-by <-**
- [[FileArtifactResolver.LoadPauseAdjustmentsInternal]]

