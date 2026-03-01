---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/AmsAppDataPaths.cs"
access_modifier: "public"
complexity: 3
fan_in: 5
fan_out: 0
tags:
  - method
---
# AmsAppDataPaths::Resolve
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/AmsAppDataPaths.cs`


#### [[AmsAppDataPaths.Resolve]]
##### What it does:
<member name="M:Ams.Core.Common.AmsAppDataPaths.Resolve(System.String[])">
    <summary>
    Resolves a path inside the AMS app data root.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string Resolve(params string[] segments)
```

**Called-by <-**
- [[ReplState.ResolveStateFilePath]]
- [[DspConfigService.GetConfigFilePath]]
- [[BookMetadataResetService.ClearCurrentChapterState]]
- [[BookMetadataResetService.RemoveBookScopedEntries]]
- [[WorkspaceHistoryService.GetFilePath]]

