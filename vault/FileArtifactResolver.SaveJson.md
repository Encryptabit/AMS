---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 1
fan_in: 4
fan_out: 1
tags:
  - method
---
# FileArtifactResolver::SaveJson
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`


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

