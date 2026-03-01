---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaInvocationContext.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
---
# MfaInvocationContext::BeginScope
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaInvocationContext.cs`


#### [[MfaInvocationContext.BeginScope]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IDisposable BeginScope(string label)
```

**Called-by <-**
- [[PipelineService.RunChapterAsync]]

