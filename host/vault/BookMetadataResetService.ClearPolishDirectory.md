---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/BookMetadataResetService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# BookMetadataResetService::ClearPolishDirectory
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/BookMetadataResetService.cs`


#### [[BookMetadataResetService.ClearPolishDirectory]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.BookMetadataResetService.ClearPolishDirectory(System.String)">
    <summary>
    Removes the .polish/ directory (staging queue JSON + undo backups) from the workspace.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool ClearPolishDirectory(string workingDirectory)
```

**Called-by <-**
- [[BookMetadataResetService.ResetCurrentBook]]

