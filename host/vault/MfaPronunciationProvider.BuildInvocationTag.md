---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# MfaPronunciationProvider::BuildInvocationTag
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`

## Summary
**It formats a contextual identifier string for MFA G2P logging.**

`BuildInvocationTag` creates a stable log prefix for a G2P run using the numeric `invocationId` and optional ambient scope metadata from `MfaInvocationContext.Label`. If the scope label is null/whitespace it returns `"[g2p#{id}]"`; otherwise it returns `"[g2p#{id}|{label}]"`, enabling per-invocation and per-scope log correlation.


#### [[MfaPronunciationProvider.BuildInvocationTag]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildInvocationTag(int invocationId)
```

**Called-by <-**
- [[MfaPronunciationProvider.RunG2pWithProgressAsync]]

