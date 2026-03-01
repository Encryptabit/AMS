---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# PhonemeComparer::Equals
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Checks whether two phoneme variants represent the same sequence after normalization.**

PhonemeComparer.Equals performs normalized, case-insensitive equality for phoneme strings. It first rejects null/whitespace inputs, then compares `Normalize(a)` and `Normalize(b)` with `StringComparison.OrdinalIgnoreCase`. `Normalize` canonicalizes spacing via tokenization/rejoin, so equivalent phoneme sequences with different whitespace formatting compare equal.


#### [[PhonemeComparer.Equals]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static bool Equals(string a, string b)
```

**Calls ->**
- [[PhonemeComparer.Normalize]]

**Called-by <-**
- [[TranscriptAligner.HasExactPhonemeMatch]]

