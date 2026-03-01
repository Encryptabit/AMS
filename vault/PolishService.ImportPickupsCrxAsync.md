---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "public"
complexity: 15
fan_in: 0
fan_out: 4
tags:
  - method
  - danger/high-complexity
---
# PolishService::ImportPickupsCrxAsync
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.


#### [[PolishService.ImportPickupsCrxAsync]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishService.ImportPickupsCrxAsync(System.String,System.Collections.Generic.IReadOnlyList{Ams.Workstation.Server.Models.CrxPickupTarget},System.IProgress{System.ValueTuple{System.String,System.Double}},System.Threading.CancellationToken)">
    <summary>
    Imports a pickup recording using CRX-driven positional pairing.
    Checks cached artifacts for staleness before re-processing.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<Dictionary<string, List<CrossChapterPickupMatch>>> ImportPickupsCrxAsync(string pickupFilePath, IReadOnlyList<CrxPickupTarget> crxTargets, IProgress<(string Status, double Progress)> progress = null, CancellationToken ct = default(CancellationToken))
```

**Calls ->**
- [[PickupMatchingService.MatchPickupCrxAsync]]
- [[PolishService.ComputeCrxFingerprint]]
- [[PolishService.TryReadMatchedArtifacts]]
- [[PolishService.WriteMatchedArtifacts]]

