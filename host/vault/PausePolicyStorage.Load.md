---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PausePolicyStorage.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# PausePolicyStorage::Load
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PausePolicyStorage.cs`

## Summary
**Loads a pause policy from JSON on disk and maps it into a `PausePolicy` instance.**

`PausePolicyStorage.Load` performs defensive loading of a pause-policy JSON file and converts it into a runtime `PausePolicy`. It validates `path` (`string.IsNullOrWhiteSpace`), verifies existence (`File.Exists`), reads text via `File.ReadAllText`, then deserializes `PausePolicySnapshot` using shared camelCase `JsonOptions`. If deserialization returns `null`, it throws `InvalidOperationException`; otherwise it returns `snapshot.ToPolicy()` for final object materialization.


#### [[PausePolicyStorage.Load]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PausePolicy Load(string path)
```

**Calls ->**
- [[PausePolicySnapshot.ToPolicy]]

**Called-by <-**
- [[PausePolicyResolver.Resolve]]
- [[FileArtifactResolver.LoadPausePolicy]]

