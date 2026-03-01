---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "public"
complexity: 26
fan_in: 0
fan_out: 8
tags:
  - method
  - danger/high-complexity
  - llm/async
  - llm/data-access
  - llm/utility
  - llm/error-handling
---
# MfaPronunciationProvider::GetPronunciationsAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`

> [!danger] High Complexity (26)
> Cyclomatic complexity: 26. Consider refactoring into smaller methods.

## Summary
**It resolves pronunciations for input lexemes by combining cached phoneme data with on-demand MFA G2P generation and cache backfill.**

`GetPronunciationsAsync` normalizes/deduplicates requested lexemes, fetches cached pronunciations via `PronunciationLexiconCache.GetManyAsync`, and short-circuits if everything is already cached. For cache misses, it decomposes lexemes into atomic words (`SplitLexemeIntoWords`), runs MFA G2P generation through `RunG2pWithProgressAsync`, parses `g2p.txt` into normalized pronunciation variants (`NormalizeVariantKey`), and composes lexeme-level pronunciations (`ComposeLexemePronunciations`). It then merges generated entries back into the cache with `MergeAsync`, returns combined cached+generated results via `MergePronunciationMaps`, and degrades gracefully on failures (non-zero exit/missing output/exceptions) by logging debug diagnostics and returning cached-only data, with best-effort temp-directory cleanup in `finally`.


#### [[MfaPronunciationProvider.GetPronunciationsAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<IReadOnlyDictionary<string, string[]>> GetPronunciationsAsync(IEnumerable<string> words, CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaPronunciationProvider.ComposeLexemePronunciations]]
- [[MfaPronunciationProvider.MergePronunciationMaps]]
- [[MfaPronunciationProvider.NormalizeVariantKey]]
- [[MfaPronunciationProvider.RunG2pWithProgressAsync]]
- [[PronunciationLexiconCache.GetManyAsync]]
- [[PronunciationLexiconCache.MergeAsync]]
- [[Log.Debug]]
- [[PronunciationHelper.SplitLexemeIntoWords]]

