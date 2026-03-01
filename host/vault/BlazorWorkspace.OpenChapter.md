---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/BlazorWorkspace.cs"
access_modifier: "public"
complexity: 5
fan_in: 2
fan_out: 2
tags:
  - method
---
# BlazorWorkspace::OpenChapter
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/BlazorWorkspace.cs`


#### [[BlazorWorkspace.OpenChapter]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.BlazorWorkspace.OpenChapter(Ams.Core.Runtime.Workspace.ChapterOpenOptions)">
    <inheritdoc />
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterContextHandle OpenChapter(ChapterOpenOptions options)
```

**Calls ->**
- [[ChapterManager.CreateContext]]
- [[BlazorWorkspace.ResolveDefaultBookIndex]]

**Called-by <-**
- [[BlazorWorkspace.SelectChapter]]
- [[BlazorWorkspace.TryGetHydratedTranscript]]

