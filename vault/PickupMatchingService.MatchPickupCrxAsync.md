---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 10
tags:
  - method
---
# PickupMatchingService::MatchPickupCrxAsync
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs`


#### [[PickupMatchingService.MatchPickupCrxAsync]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PickupMatchingService.MatchPickupCrxAsync(System.String,System.Collections.Generic.IReadOnlyList{Ams.Workstation.Server.Models.CrxPickupTarget},System.Threading.CancellationToken)">
    <summary>
    Processes a pickup session recording using CRX-driven positional pairing.
    ASR + MFA run on the full WAV, then utterances are segmented by silence gaps
    and paired with CRX targets in ErrorNumber order.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<List<PickupMatch>> MatchPickupCrxAsync(string pickupFilePath, IReadOnlyList<CrxPickupTarget> crxTargets, CancellationToken ct)
```

**Calls ->**
- [[AsrAudioPreparer.PrepareForAsr]]
- [[AsrProcessor.TranscribeBufferAsync]]
- [[AudioProcessor.Decode]]
- [[PickupMatchingService.BuildAsrOptionsAsync]]
- [[PickupMatchingService.PairSegmentsToTargets]]
- [[PickupMatchingService.SegmentUtterances]]
- [[PickupMatchingService.TryReadNamedAsrCache]]
- [[PickupMatchingService.WriteNamedAsrCache]]
- [[PickupMatchingService.WriteNamedMfaCache]]
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

**Called-by <-**
- [[PolishService.ImportPickupsCrxAsync]]

