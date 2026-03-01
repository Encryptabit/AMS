---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaService.cs"
access_modifier: "public"
complexity: 5
fan_in: 3
fan_out: 3
tags:
  - method
  - llm/async
  - llm/validation
  - llm/error-handling
---
# MfaService::GeneratePronunciationsAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaService.cs`

## Summary
**It builds and dispatches an MFA G2P command to generate pronunciations for an OOV list into an output lexicon file.**

`GeneratePronunciationsAsync` validates `context` and requires both `context.OovListPath` and `context.G2pOutputPath`, throwing `ArgumentException` when either is missing. It selects a G2P model from `context.G2pModel` or falls back to `DefaultG2pModel`, then builds MFA `g2p` arguments using `QuoteRequired` for required file paths and `Quote` for the model token. Execution is delegated to `RunCommandAsync(G2pCommand, args, context.WorkingDirectory, cancellationToken)` and returned as `Task<MfaCommandResult>`.


#### [[MfaService.GeneratePronunciationsAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<MfaCommandResult> GeneratePronunciationsAsync(MfaChapterContext context, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[MfaService.Quote]]
- [[MfaService.QuoteRequired]]
- [[MfaService.RunCommandAsync]]

**Called-by <-**
- [[MfaPronunciationProvider.RunG2pWithProgressAsync]]
- [[MfaWorkflow.RunChapterAsync]]
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

