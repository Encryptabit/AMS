---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaService.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 3
tags:
  - method
  - llm/async
  - llm/validation
  - llm/error-handling
---
# MfaService::AddWordsAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaService.cs`

## Summary
**It runs MFA’s `model add_words` command to merge generated pronunciations into a base dictionary model.**

`AddWordsAsync` validates `context` and enforces that `context.G2pOutputPath` is provided, throwing `ArgumentException` otherwise. It resolves the base dictionary from `context.DictionaryModel` with fallback to `DefaultDictionaryModel`, then builds arguments as `<dictionary> <g2pOutput>` using `Quote`/`QuoteRequired`. The method executes MFA `model add_words` by delegating to `RunCommandAsync(AddWordsCommand, args, context.WorkingDirectory, cancellationToken)`.


#### [[MfaService.AddWordsAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<MfaCommandResult> AddWordsAsync(MfaChapterContext context, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[MfaService.Quote]]
- [[MfaService.QuoteRequired]]
- [[MfaService.RunCommandAsync]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

