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
# PausePolicyStorage::Save
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PausePolicyStorage.cs`

## Summary
**Validates and serializes a pause policy snapshot to disk at the specified path.**

`PausePolicyStorage.Save` persists a `PausePolicy` to JSON using shared camelCase/indented `JsonOptions`. It validates inputs (`path` must be non-empty and `policy` non-null), creates the parent directory when needed (`Directory.CreateDirectory`), converts the model with `PausePolicySnapshot.FromPolicy(policy)`, serializes via `JsonSerializer.Serialize`, and writes the file with `File.WriteAllText`. The method is synchronous and lets underlying IO/serialization exceptions propagate.


#### [[PausePolicyStorage.Save]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Save(string path, PausePolicy policy)
```

**Calls ->**
- [[PausePolicySnapshot.FromPolicy]]

**Called-by <-**
- [[ValidateCommand.CreateTimingInitCommand]]
- [[FileArtifactResolver.SavePausePolicy]]

