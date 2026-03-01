---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BatchOperationService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
---
# BatchOperationService::GetAvailableChapters
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BatchOperationService.cs`


#### [[BatchOperationService.GetAvailableChapters]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.BatchOperationService.GetAvailableChapters">
    <summary>
    Returns all available chapters from the workspace with selection state.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public List<BatchTarget> GetAvailableChapters()
```

**Calls ->**
- [[BlazorWorkspace.GetStemForChapter]]

