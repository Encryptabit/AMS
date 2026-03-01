---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 6
fan_in: 2
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptAligner::SubCost
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Assigns substitution cost between book and ASR tokens based on exact/equivalent, near-match, and phoneme-aware similarity rules.**

SubCost computes substitution penalty tiers for token alignment using lexical, equivalence-map, and phoneme signals. It returns `0.0` for exact/equivalent tokens (`Equivalent`) or exact phoneme matches (`HasExactPhonemeMatch`). It returns `0.3` for near matches when `LevLe1(bookTok, asrTok)` is true, or when soft phoneme matching is explicitly enabled (`phonemeSoftThreshold <= 1.0`) and `HasSoftPhonemeMatch` passes the threshold. All remaining pairs are treated as full substitutions with cost `1.0`.


#### [[TranscriptAligner.SubCost]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double SubCost(string bookTok, string asrTok, IReadOnlyDictionary<string, string> equiv, string[] bookPhonemes = null, string[] asrPhonemes = null, double phonemeSoftThreshold = 1.01)
```

**Calls ->**
- [[TranscriptAligner.Equivalent]]
- [[TranscriptAligner.HasExactPhonemeMatch]]
- [[TranscriptAligner.HasSoftPhonemeMatch]]
- [[TranscriptAligner.LevLe1]]

**Called-by <-**
- [[TranscriptAligner.AlignWindows]]
- [[TxAlignTests.SubCost_DefaultDisablesSoftPhonemeMatching]]

