---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 7
tags:
  - method
---
# PickupMatchingService::MatchSinglePickupAsync
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs`


#### [[PickupMatchingService.MatchSinglePickupAsync]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PickupMatchingService.MatchSinglePickupAsync(System.String,Ams.Core.Artifacts.Hydrate.HydratedSentence,System.Threading.CancellationToken)">
    <summary>
    Simplified matching for individual pickup files (one per sentence).
    Runs ASR on the entire file and creates a match with the specified target sentence.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<PickupMatch> MatchSinglePickupAsync(string pickupFilePath, HydratedSentence targetSentence, CancellationToken ct)
```

**Calls ->**
- [[AsrAudioPreparer.PrepareForAsr]]
- [[LevenshteinMetrics.Similarity_2]]
- [[AsrProcessor.TranscribeBufferAsync]]
- [[AudioProcessor.Decode]]
- [[PickupMatchingService.BuildAsrOptionsAsync]]
- [[PickupMatchingService.ExtractFullText]]
- [[PickupMatchingService.NormalizeForMatch]]

