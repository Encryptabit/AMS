---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "private"
complexity: 11
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# MfaTimingMerger::TokenizeForAlignment
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`

## Summary
**Normalizes and tokenizes text into alignment-ready tokens, with optional TextGrid-specific wildcard mapping for unknown/silence markers.**

TokenizeForAlignment yields a normalized token stream from input text, exiting early for null/whitespace before and after `SafeNormalize`. It canonicalizes typographic quotes/dashes to ASCII equivalents, then delegates lexical splitting to `PronunciationHelper.ExtractPronunciationParts` to stay symmetric with MFA lab generation. Each emitted part is lowercased; empty parts are skipped. When `forTextGrid` is true, MFA special/unknown markers (`<unk>`, `unk`, `sp`, `sil`) are collapsed to the sentinel `UNK` wildcard token.


#### [[MfaTimingMerger.TokenizeForAlignment]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<string> TokenizeForAlignment(string s, bool forTextGrid)
```

**Calls ->**
- [[MfaTimingMerger.SafeNormalize]]
- [[PronunciationHelper.ExtractPronunciationParts]]

**Called-by <-**
- [[MfaTimingMerger.BuildBookTokens]]
- [[MfaTimingMerger.BuildTimedTgTokens]]

