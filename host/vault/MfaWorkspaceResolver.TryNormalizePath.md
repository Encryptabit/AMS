---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# MfaWorkspaceResolver::TryNormalizePath
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs`

## Summary
**It performs safe, non-throwing path normalization with success/failure signaling via a boolean and out parameter.**

`TryNormalizePath` validates and canonicalizes a candidate path into an absolute path. It returns `false` and sets `normalized` to `string.Empty` when input is null/whitespace, otherwise it attempts `Path.GetFullPath(path.Trim())` and returns `true` with the normalized result. Any exception during normalization is swallowed and reported as `false`.


#### [[MfaWorkspaceResolver.TryNormalizePath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryNormalizePath(string path, out string normalized)
```

**Called-by <-**
- [[MfaWorkspaceResolver.ParseWorkspaceList]]
- [[MfaWorkspaceResolver.ResolvePreferredRoot]]

