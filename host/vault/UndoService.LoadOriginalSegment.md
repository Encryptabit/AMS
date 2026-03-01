---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
---
# UndoService::LoadOriginalSegment
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs`


#### [[UndoService.LoadOriginalSegment]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.UndoService.LoadOriginalSegment(System.String)">
    <summary>
    Decodes the backup WAV file back to an AudioBuffer.
    </summary>
    <returns>The original audio segment, or null if the backup file is missing.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBuffer LoadOriginalSegment(string replacementId)
```

**Calls ->**
- [[AudioProcessor.Decode]]
- [[UndoService.GetUndoRecordInternal]]

**Called-by <-**
- [[PolishService.RevertReplacementAsync]]

