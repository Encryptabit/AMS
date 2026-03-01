---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaService.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/async
  - llm/validation
  - llm/error-handling
---
# MfaService::ValidateAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaService.cs`

## Summary
**It constructs and executes an MFA `validate` command for a chapter corpus with safe defaults and validation-focused flags.**

`ValidateAsync` validates `context` for null, resolves dictionary/acoustic model names with fallbacks to `DefaultDictionaryModel` and `DefaultAcousticModel`, and builds MFA `validate` CLI arguments using `QuoteRequired(context.CorpusDirectory)` plus quoted model identifiers. It conditionally appends `--single_speaker` and always appends `--no_train` to avoid monophone training on tiny corpora. The method delegates execution to `RunCommandAsync(ValidateCommand, args, context.WorkingDirectory, cancellationToken)` and returns the resulting `Task<MfaCommandResult>`.


#### [[MfaService.ValidateAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<MfaCommandResult> ValidateAsync(MfaChapterContext context, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[MfaService.Quote]]
- [[MfaService.QuoteRequired]]
- [[MfaService.RunCommandAsync]]

**Called-by <-**
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

