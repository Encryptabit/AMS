---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 1
fan_in: 4
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FileArtifactResolver::SaveJson
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Validates, serializes, and writes a typed JSON artifact to disk at the specified path.**

`SaveJson<T>` is a generic synchronous writer for class payloads that enforces non-null input with `ArgumentNullException.ThrowIfNull(payload)`. It ensures the destination directory exists via `EnsureDirectory(path)`, serializes using shared `JsonOptions` (`JsonSerializer.Serialize(payload, JsonOptions)`), and writes the full JSON using `File.WriteAllText`. The method centralizes common artifact save behavior for multiple resolver save APIs.


#### [[FileArtifactResolver.SaveJson]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void SaveJson<T>(string path, T payload) where T : class
```

**Calls ->**
- [[FileArtifactResolver.EnsureDirectory]]

**Called-by <-**
- [[FileArtifactResolver.SaveAnchors]]
- [[FileArtifactResolver.SaveAsr]]
- [[FileArtifactResolver.SaveHydratedTranscript]]
- [[FileArtifactResolver.SaveTranscript]]

