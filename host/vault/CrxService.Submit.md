---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 7
tags:
  - method
---
# CrxService::Submit
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.Submit]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.CrxService.Submit(System.String,Ams.Workstation.Server.Models.CrxSubmitRequest)">
    <summary>
    Submit a CRX entry: export audio and record metadata to JSON and Excel.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public CrxSubmitResult Submit(string chapterName, CrxSubmitRequest request)
```

**Calls ->**
- [[AudioExportService.ExportSegment]]
- [[CrxService.AppendCrxEntry]]
- [[CrxService.AppendOrUpdateJsonEntry]]
- [[CrxService.EnsureExcelReady]]
- [[CrxService.FormatTimecode]]
- [[CrxService.TryDeleteExportedFile]]
- [[CrxService.TryRemoveJsonEntry]]

