---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseModels.cs"
access_modifier: "public"
complexity: 1
fan_in: 6
fan_out: 0
tags:
  - method
---
# PausePolicyPresets::House
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseModels.cs`


#### [[PausePolicyPresets.House]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PausePolicy House()
```

**Called-by <-**
- [[ValidateCommand.CreateTimingInitCommand]]
- [[PausePolicyResolver.Resolve]]
- [[PauseMapBuilder.Build]]
- [[FileArtifactResolver.LoadPausePolicy]]
- [[ChapterDocuments..ctor]]
- [[PauseDynamicsServiceTests.PlanTransforms_CompressesSentencePauseOutsideWindow]]

