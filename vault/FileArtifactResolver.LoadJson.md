---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 0
tags:
  - method
---
# FileArtifactResolver::LoadJson
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`


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

