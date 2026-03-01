---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/utility
---
# IPauseDynamicsService::Execute
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Declares the contract for running the full pause analysis/planning/application pipeline for a chapter.**

In `IPauseDynamicsService`, `Execute` is an interface method declaration with no implementation body. The signature defines an end-to-end pause-dynamics operation that takes transcript/book context, optional hydrated transcript data, policy, and optional intra-sentence silence controls, and returns a composed `PauseDynamicsResult`.


#### [[IPauseDynamicsService.Execute]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
PauseDynamicsResult Execute(TranscriptIndex transcript, BookIndex bookIndex, HydratedTranscript hydrated, PausePolicy policy, IReadOnlyList<(double Start, double End)> intraSentenceSilences = null, bool includeAllIntraSentenceGaps = true)
```

