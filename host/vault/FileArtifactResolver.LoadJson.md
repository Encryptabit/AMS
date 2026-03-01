---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/error-handling
---
# FileArtifactResolver::LoadJson
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Loads and deserializes a JSON artifact into type `T`, returning a default value when the file does not exist.**

`LoadJson<T>` is a generic file loader that first checks `File.Exists(path)` and returns `default` when the artifact is missing. If present, it reads the full file (`File.ReadAllText`) and deserializes using `JsonSerializer.Deserialize<T>(json, JsonOptions)`. The method performs no argument validation or exception suppression, so read/parse failures propagate to callers.


#### [[FileArtifactResolver.LoadJson]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static T LoadJson<T>(string path)
```

**Called-by <-**
- [[FileArtifactResolver.LoadAnchors]]
- [[FileArtifactResolver.LoadAsr]]
- [[FileArtifactResolver.LoadHydratedTranscript]]
- [[FileArtifactResolver.LoadTranscript]]

