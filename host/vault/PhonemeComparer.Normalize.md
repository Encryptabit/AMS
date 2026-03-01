---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# PhonemeComparer::Normalize
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Converts a phoneme variant string into a stable, single-space-delimited token sequence for reliable equality checks.**

`Normalize` is an expression-bodied helper in `TranscriptAligner.PhonemeComparer` that returns `string.Join(' ', Tokenize(value))` to canonicalize phoneme text spacing. Since `Tokenize` splits on `' '` with `StringSplitOptions.RemoveEmptyEntries`, it removes empty tokens and collapses repeated/edge whitespace to single separators. The method itself does not alter case or content beyond token-boundary normalization.


#### [[PhonemeComparer.Normalize]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string Normalize(string value)
```

**Calls ->**
- [[PhonemeComparer.Tokenize]]

**Called-by <-**
- [[PhonemeComparer.Equals]]

