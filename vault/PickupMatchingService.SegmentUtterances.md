---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs"
access_modifier: "private"
complexity: 10
fan_in: 1
fan_out: 1
tags:
  - method
---
# PickupMatchingService::SegmentUtterances
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs`


#### [[PickupMatchingService.SegmentUtterances]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PickupMatchingService.SegmentUtterances(Ams.Core.Asr.AsrToken[])">
    <summary>
    Segments ASR tokens into utterances based on silence gaps.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<PickupSegment> SegmentUtterances(AsrToken[] tokens)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[PickupMatchingService.MatchPickupCrxAsync]]

