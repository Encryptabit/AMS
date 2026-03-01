---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/AmsAppDataPaths.cs"
access_modifier: "public"
complexity: 3
fan_in: 5
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AmsAppDataPaths::Resolve
**Path**: `Projects/AMS/host/Ams.Core/Common/AmsAppDataPaths.cs`

## Summary
**Construct an absolute path under the AMS app-data root from a variable list of path segments.**

`Resolve(params string[] segments)` builds a path anchored at `RootPath` for AMS app data. It validates input with `ArgumentNullException.ThrowIfNull(segments)`, initializes `path` to `RootPath`, then iterates each segment and skips null/whitespace entries. Non-empty segments are appended using `Path.Combine`, and the final combined path is returned.


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

