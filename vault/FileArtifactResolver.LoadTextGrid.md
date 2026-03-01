---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 2
tags:
  - method
---
# FileArtifactResolver::LoadTextGrid
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`


#### [[FileArtifactResolver.LoadTextGrid]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public TextGridDocument LoadTextGrid(ChapterContext context)
```

**Calls ->**
- [[TextGridParser.ParseWordIntervals]]
- [[FileArtifactResolver.GetTextGridPath]]

