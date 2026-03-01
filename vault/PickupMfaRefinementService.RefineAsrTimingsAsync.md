---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs"
access_modifier: "public"
complexity: 19
fan_in: 1
fan_out: 19
tags:
  - method
  - danger/high-complexity
---
# PickupMfaRefinementService::RefineAsrTimingsAsync
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs`

> [!danger] High Complexity (19)
> Cyclomatic complexity: 19. Consider refactoring into smaller methods.


#### [[PickupMfaRefinementService.RefineAsrTimingsAsync]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PickupMfaRefinementService.RefineAsrTimingsAsync(System.String,Ams.Core.Asr.AsrResponse,System.Threading.CancellationToken)">
    <summary>
    Refines ASR token timings using full-file MFA forced alignment.
    Falls back to the original ASR response on any MFA failure.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<AsrResponse> RefineAsrTimingsAsync(string pickupFilePath, AsrResponse asrResponse, CancellationToken ct)
```

**Calls ->**
- [[MfaService.AddWordsAsync]]
- [[MfaService.AlignAsync]]
- [[MfaService.GeneratePronunciationsAsync]]
- [[MfaService.ValidateAsync]]
- [[MfaProcessSupervisor.EnsureReadyAsync]]
- [[Log.Debug]]
- [[Log.Warn]]
- [[TextGridParser.ParseWordIntervals]]
- [[PickupMfaRefinementService.AlignMfaWordsToAsrTokens]]
- [[PickupMfaRefinementService.ApplyRefinedTimings]]
- [[PickupMfaRefinementService.BuildAlignmentWords]]
- [[PickupMfaRefinementService.ComputeMfaCacheKey]]
- [[PickupMfaRefinementService.EnsureLabContentAsync]]
- [[PickupMfaRefinementService.EnsureStagedPickupWav]]
- [[PickupMfaRefinementService.FindOovListFile]]
- [[PickupMfaRefinementService.FindTextGridFile]]
- [[PickupMfaRefinementService.LogMfaResult]]
- [[PickupMfaRefinementService.TryReadAsrResponseCache]]
- [[PickupMfaRefinementService.WriteAsrResponseCache]]

**Called-by <-**
- [[PickupMatchingService.MatchPickupCrxAsync]]

