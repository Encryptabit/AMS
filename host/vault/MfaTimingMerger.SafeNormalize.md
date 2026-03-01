---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# MfaTimingMerger::SafeNormalize
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`

## Summary
**Cleans and compatibility-normalizes input text while defensively handling invalid Unicode data.**

SafeNormalize sanitizes potentially malformed Unicode text before alignment tokenization. It returns early for null/empty input, then scans characters, preserving valid surrogate pairs, dropping unpaired surrogates, and removing non-whitespace control characters while copying others into a `StringBuilder`. If the cleaned result is empty it returns `string.Empty`; otherwise it attempts Unicode normalization with `NormalizationForm.FormKC`. If normalization throws `ArgumentException`, it falls back to returning the cleaned (unnormalized) string.


#### [[MfaTimingMerger.SafeNormalize]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string SafeNormalize(string input)
```

**Called-by <-**
- [[MfaTimingMerger.TokenizeForAlignment]]

