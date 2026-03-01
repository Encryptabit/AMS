---
namespace: "Ams.Core.Pipeline"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Pipeline/SentenceRefinementService.cs"
access_modifier: "public"
complexity: 23
fan_in: 1
fan_out: 4
tags:
  - method
  - danger/high-complexity
  - llm/async
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# SentenceRefinementService::RefineAsync
**Path**: `Projects/AMS/host/Ams.Core/Pipeline/SentenceRefinementService.cs`

> [!danger] High Complexity (23)
> Cyclomatic complexity: 23. Consider refactoring into smaller methods.

## Summary
**Asynchronously produces refined sentence timing windows from transcript/ASR indices using Aeneas alignment with optional silence-based end-boundary adjustment.**

RefineAsync validates `audioPath`, then builds per-sentence alignment text by mapping each `tx.Sentences` `ScriptRange` to clamped ASR token indices and joining non-empty `asr.GetWord(...)` tokens while tracking `(sentenceId,startIdx,endIdx)`. It awaits `RunAeneasAsync` for fragment begin/end timings and optionally awaits `DetectSilencesAsync`, emitting zeroed non-fragment timings when script ranges/words are unavailable. For fragment-backed sentences, it optionally snaps `end` to the first `silence_end` between fragment end and next fragment begin, enforces bounds (`end >= start + 0.05`, `end <= nextBegin`), and finally removes overlaps by shortening prior timings via `WithEnd`.


#### [[SentenceRefinementService.RefineAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<IReadOnlyList<SentenceRefined>> RefineAsync(string audioPath, TranscriptIndex tx, AsrResponse asr, string language = "eng", bool useSilence = true, double silenceThresholdDb = -30, double silenceMinDurationSec = 0.1)
```

**Calls ->**
- [[SentenceTiming.WithEnd]]
- [[AsrResponse.GetWord]]
- [[SentenceRefinementService.DetectSilencesAsync]]
- [[SentenceRefinementService.RunAeneasAsync]]

**Called-by <-**
- [[RefineSentencesCommand.RunAsync]]

