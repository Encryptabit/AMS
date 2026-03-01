---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# MfaWorkspaceResolver::ParseWorkspaceList
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs`

## Summary
**It converts a delimited workspace path string into a filtered list of normalized filesystem paths.**

`ParseWorkspaceList` parses a path-list string (typically env-var content) separated by `Path.PathSeparator`, ignoring empty/whitespace entries via `RemoveEmptyEntries | TrimEntries`. Each entry is validated/normalized through `TryNormalizePath`; only successfully normalized paths are kept in order in a `List<string>`. If input is null/whitespace, it returns `Array.Empty<string>()`.


#### [[MfaWorkspaceResolver.ParseWorkspaceList]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<string> ParseWorkspaceList(string raw)
```

**Calls ->**
- [[MfaWorkspaceResolver.TryNormalizePath]]

**Called-by <-**
- [[MfaWorkspaceResolver.ResolvePreferredRoot]]
- [[MfaWorkspaceResolver.ResolveWorkspaceRoots]]

