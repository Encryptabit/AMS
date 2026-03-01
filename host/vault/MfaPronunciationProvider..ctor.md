---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/factory
---
# MfaPronunciationProvider::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`

## Summary
**It constructs the pronunciation provider by wiring the MFA service instance and effective G2P model name.**

The constructor initializes core dependencies with null-coalescing defaults: `_mfaService` is set to the provided `mfaService` or a new `MfaService` instance, and `_g2pModel` is set to the provided `g2pModel` or `MfaService.DefaultG2pModel`. This allows explicit dependency injection while preserving a self-contained default runtime configuration.


#### [[MfaPronunciationProvider..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public MfaPronunciationProvider(MfaService mfaService = null, string g2pModel = null)
```

