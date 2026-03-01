---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptAligner::HasSoftPhonemeMatch
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Checks whether any phoneme-variant pair is similar enough to meet a configurable soft-match threshold.**

HasSoftPhonemeMatch performs thresholded similarity matching across all non-empty phoneme-variant pairs from book and ASR inputs. After guarding for null/empty arrays, it tokenizes each candidate with `PhonemeComparer.Tokenize`, skips empty tokenizations, and computes similarity via `PhonemeComparer.Similarity`. It tracks the best similarity seen and short-circuits as soon as `best >= threshold`; otherwise returns false after exhaustive comparison. This provides approximate phoneme matching rather than exact equality.


#### [[TranscriptAligner.HasSoftPhonemeMatch]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool HasSoftPhonemeMatch(string[] bookPhonemes, string[] asrPhonemes, double threshold)
```

**Calls ->**
- [[PhonemeComparer.Similarity]]
- [[PhonemeComparer.Tokenize]]

**Called-by <-**
- [[TranscriptAligner.SubCost]]

