---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
---
# IPauseDynamicsService::Execute
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`


#### [[IPauseDynamicsService.Execute]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
PauseDynamicsResult Execute(TranscriptIndex transcript, BookIndex bookIndex, HydratedTranscript hydrated, PausePolicy policy, IReadOnlyList<(double Start, double End)> intraSentenceSilences = null, bool includeAllIntraSentenceGaps = true)
```

