---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
---
# MfaWorkspaceResolver::EnsureWorkspace
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs`

## Summary
**It guarantees a workspace directory exists and returns its absolute path.**

`EnsureWorkspace` materializes the target directory by calling `Directory.CreateDirectory(path)` and returns `Path.GetFullPath(path)` so callers receive a normalized absolute workspace path. It has side effects on the filesystem and no internal branching.


#### [[MfaWorkspaceResolver.EnsureWorkspace]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string EnsureWorkspace(string path)
```

**Called-by <-**
- [[MfaWorkspaceResolver.ResolvePreferredRoot]]

