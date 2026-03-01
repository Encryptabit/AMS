---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/AudioExportService.cs"
access_modifier: "public"
complexity: 7
fan_in: 1
fan_out: 2
tags:
  - method
---
# AudioExportService::ExportSegment
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/AudioExportService.cs`


#### [[AudioExportService.ExportSegment]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.AudioExportService.ExportSegment(System.Double,System.Double,System.Int32)">
    <summary>
    Export an audio segment to the CRX folder.
    Uses AudioProcessor.Trim (FFmpeg atrim) for segment extraction,
    then ToWavStream() on the trimmed buffer.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ExportResult ExportSegment(double startSec, double endSec, int paddingMs = 0)
```

**Calls ->**
- [[AudioBuffer.ToWavStream]]
- [[AudioProcessor.Trim]]

**Called-by <-**
- [[CrxService.Submit]]

