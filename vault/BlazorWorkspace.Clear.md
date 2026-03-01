---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BlazorWorkspace.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 2
tags:
  - method
---
# BlazorWorkspace::Clear
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BlazorWorkspace.cs`


#### [[BlazorWorkspace.Clear]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.BlazorWorkspace.Clear">
    <summary>
    Clears all state, resetting to initial values.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Clear()
```

**Calls ->**
- [[ChapterContextHandle.Dispose]]
- [[BlazorWorkspace.SavePersistedState]]

