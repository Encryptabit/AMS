---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaService.cs"
access_modifier: "public"
complexity: 9
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/async
  - llm/validation
  - llm/error-handling
---
# MfaService::AlignAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaService.cs`

## Summary
**It constructs and runs an MFA alignment command for a chapter corpus with optional decoding and cleanup parameters.**

`AlignAsync` validates `context`, then builds MFA `align` arguments from corpus path, dictionary source, acoustic model, and output directory with required quoting via `QuoteRequired`. Dictionary resolution prefers `context.CustomDictionaryPath`; otherwise it uses `context.DictionaryModel` or `DefaultDictionaryModel`, while acoustic falls back to `DefaultAcousticModel` when unspecified. It conditionally appends tuning/behavior flags (`--beam`, `--retry_beam`, `--single_speaker`, `--clean`) and delegates execution to `RunCommandAsync(AlignCommandName, args, context.WorkingDirectory, cancellationToken)`.


#### [[MfaService.AlignAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<MfaCommandResult> AlignAsync(MfaChapterContext context, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[MfaService.QuoteRequired]]
- [[MfaService.RunCommandAsync]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

