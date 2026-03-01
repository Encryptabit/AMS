---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# ScriptValidator::ExtractWordsFromAsrResponse
**Path**: `Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

## Summary
**Transforms raw ASR output into normalized word-alignment records (with optional timing metadata) for the validator’s downstream alignment step.**

`ExtractWordsFromAsrResponse` builds a `List<WordAlignment>` by iterating `asrResponse.WordCount`, reading each token text via `asrResponse.GetWord(i)`, and normalizing it with `TextNormalizer.Normalize` using `_options.ExpandContractions` and `_options.RemoveNumbers`. It drops entries whose normalized form is empty, so punctuation/filtered tokens are excluded from alignment input. For retained words, it conditionally maps timing (`StartTime`, `EndTime = StartTime + Duration`) from `asrResponse.Tokens[i]` only when `HasWordTimings` is true and the token index is in range; otherwise both times default to `0`, while `OriginalWord` preserves the pre-normalized text.


#### [[ScriptValidator.ExtractWordsFromAsrResponse]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private List<ScriptValidator.WordAlignment> ExtractWordsFromAsrResponse(AsrResponse asrResponse)
```

**Calls ->**
- [[AsrResponse.GetWord]]
- [[TextNormalizer.Normalize]]

**Called-by <-**
- [[ScriptValidator.Validate]]

