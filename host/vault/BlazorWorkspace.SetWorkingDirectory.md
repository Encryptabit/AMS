---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/BlazorWorkspace.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 4
tags:
  - method
---
# BlazorWorkspace::SetWorkingDirectory
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/BlazorWorkspace.cs`


#### [[BlazorWorkspace.SetWorkingDirectory]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.BlazorWorkspace.SetWorkingDirectory(System.String)">
    <summary>
    Sets the working directory and initializes the workspace.
    </summary>
    <param name="path">Path to the working directory containing book-index.json.</param>
    <returns>True if successful, false if path is invalid.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool SetWorkingDirectory(string path)
```

**Calls ->**
- [[ChapterContextHandle.Dispose]]
- [[BlazorWorkspace.BuildDescriptor]]
- [[BlazorWorkspace.LoadChaptersFromIndex]]
- [[BlazorWorkspace.SavePersistedState]]

**Called-by <-**
- [[BlazorWorkspace.LoadPersistedState]]

