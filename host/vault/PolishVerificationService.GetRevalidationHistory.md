---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
---
# PolishVerificationService::GetRevalidationHistory
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs`


#### [[PolishVerificationService.GetRevalidationHistory]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishVerificationService.GetRevalidationHistory(System.String)">
    <summary>
    Returns the list of recent re-validations for a chapter (in-memory cache).
    </summary>
    <param name="chapterStem">The chapter stem identifier.</param>
    <returns>Read-only list of revalidation results.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<RevalidationResult> GetRevalidationHistory(string chapterStem)
```

