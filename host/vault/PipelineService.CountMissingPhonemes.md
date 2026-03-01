---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# PipelineService::CountMissingPhonemes
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

## Summary
**Counts how many words in a `BookIndex` still lack phoneme data for valid normalized lookup tokens.**

`CountMissingPhonemes` computes a filtered count over `index.Words` using `Enumerable.Count`. A word is considered missing only when `word.Phonemes` is null/empty (`is not { Length: > 0 }`) and its text normalizes to a non-empty lookup token via `PronunciationHelper.NormalizeForLookup(word.Text)`. This intentionally ignores non-lexical tokens while measuring actionable phoneme gaps, and its result is used by `EnsurePhonemesAsync` before/after backfill to detect improvement.


#### [[PipelineService.CountMissingPhonemes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int CountMissingPhonemes(BookIndex index)
```

**Calls ->**
- [[PronunciationHelper.NormalizeForLookup]]

**Called-by <-**
- [[PipelineService.EnsurePhonemesAsync]]

