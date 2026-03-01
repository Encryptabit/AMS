---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaInvocationContext.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# RestoreScope::Dispose
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaInvocationContext.cs`

## Summary
**It restores the prior invocation label once when the scope ends.**

`Dispose` is idempotent via the `_disposed` guard: subsequent calls return immediately. On first invocation it restores the ambient async-local label by assigning `CurrentLabel.Value = _previous`, then flips `_disposed = true` to prevent repeated restoration.


#### [[RestoreScope.Dispose]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Dispose()
```

