---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
---
# PolishVerificationService::SyncToProofAsync
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs`


#### [[PolishVerificationService.SyncToProofAsync]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishVerificationService.SyncToProofAsync(System.String,System.Int32,System.Boolean)">
    <summary>
    Syncs a sentence's fix status to the Proof area after verification.
    When a fix passes re-validation and the user accepts it, the sentence status
    automatically updates in the Proof area via <see cref="T:Ams.Workstation.Server.Services.ReviewedStatusService"/>.
    </summary>
    <param name="chapterStem">The chapter stem identifier.</param>
    <param name="sentenceId">The sentence that was fixed.</param>
    <param name="passed">Whether the re-validation passed.</param>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task SyncToProofAsync(string chapterStem, int sentenceId, bool passed)
```

**Calls ->**
- [[ReviewedStatusService.SetReviewed]]

