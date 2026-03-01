---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# MfaPronunciationProvider::NormalizeVariantKey
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`

## Summary
**It converts raw pronunciation entry keys into a normalized lexeme lookup key, removing variant-index suffixes.**

`NormalizeVariantKey` canonicalizes MFA pronunciation keys by returning `string.Empty` for null/whitespace input, trimming the token, stripping numeric variant suffixes like `word(2)` via `VariantSuffixPattern`, and then applying `PronunciationHelper.NormalizeForLookup(raw)`. It guarantees a non-null result by coalescing null normalization output to `string.Empty`.


#### [[MfaPronunciationProvider.NormalizeVariantKey]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeVariantKey(string raw)
```

**Calls ->**
- [[PronunciationHelper.NormalizeForLookup]]

**Called-by <-**
- [[MfaPronunciationProvider.GetPronunciationsAsync]]

