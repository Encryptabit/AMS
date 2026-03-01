---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/error-handling
  - llm/validation
---
# FileArtifactResolver::LoadPauseAdjustmentsInternal
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Safely loads a pause-adjustments document from disk, returning `null` when the file is absent or unreadable.**

`LoadPauseAdjustmentsInternal` performs guarded loading of a pause-adjustments file from `path`. It returns `null` immediately if the file does not exist, otherwise it attempts `PauseAdjustmentsDocument.Load(path)` inside a `try` block and catches all exceptions to return `null` on parse/read failures. This method intentionally collapses missing-file and invalid-file cases into the same nullable result contract.


#### [[FileArtifactResolver.LoadPauseAdjustmentsInternal]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static PauseAdjustmentsDocument LoadPauseAdjustmentsInternal(string path)
```

**Calls ->**
- [[PauseAdjustmentsDocument.Load]]

**Called-by <-**
- [[FileArtifactResolver.LoadPauseAdjustments]]

