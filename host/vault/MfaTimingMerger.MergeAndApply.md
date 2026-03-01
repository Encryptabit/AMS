---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "public"
complexity: 2
fan_in: 4
fan_out: 6
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# MfaTimingMerger::MergeAndApply
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`

## Summary
**Aligns MFA TextGrid intervals to chapter book tokens and propagates merged word/sentence timings to hydrate targets while reporting alignment statistics.**

MergeAndApply orchestrates the MFA timing merge pipeline: it normalizes/builds timed TextGrid tokens (`BuildTimedTgTokens`) and chapter-scoped book tokens (`BuildBookTokens`), then globally aligns them via `Align` (Needleman–Wunsch with `unk` wildcard handling on TG tokens). It converts alignment pairs into a `bookIdx -> (start,end)` map with duplicate-book-index merges through `BuildBookTimingMap`, then applies timings to word and sentence adapters using `ApplyWordTimings` and `ApplySentenceTimings`. The method optionally emits a debug summary including token counts, alignment stats (matches/wild/ins/del), and updated entity counts, and returns those metrics in a `MergeReport`.


#### [[MfaTimingMerger.MergeAndApply]]
##### What it does:
<member name="M:Ams.Core.Processors.Alignment.Mfa.MfaTimingMerger.MergeAndApply(System.Collections.Generic.IEnumerable{Ams.Core.Processors.Alignment.Mfa.TextGridWord},System.Func{System.Int32,System.String},System.Int32,System.Int32,System.Collections.Generic.IEnumerable{Ams.Core.Processors.Alignment.Mfa.WordTarget},System.Collections.Generic.IEnumerable{Ams.Core.Processors.Alignment.Mfa.SentenceTarget},System.Action{System.String})">
    <summary>
    Aligns TextGrid tokens to book tokens and applies timings to hydrate words/sentences.
    </summary>
    <param name="textGridWords">TextGrid "word" intervals (non-empty text only)</param>
    <param name="getBookToken">Fetcher for raw book token by bookIdx</param>
    <param name="chapterStartBookIdx">Inclusive start of the chapter word window</param>
    <param name="chapterEndBookIdx">Inclusive end of the chapter word window</param>
    <param name="wordTargets">Adapters for hydrate words (provide BookIdx and a setter)</param>
    <param name="sentenceTargets">Adapters for hydrate sentences (provide book range and setter)</param>
    <param name="debugLog">Optional debug logger (e.g., s => Log.Debug(s))</param>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static MergeReport MergeAndApply(IEnumerable<TextGridWord> textGridWords, Func<int, string> getBookToken, int chapterStartBookIdx, int chapterEndBookIdx, IEnumerable<WordTarget> wordTargets, IEnumerable<SentenceTarget> sentenceTargets, Action<string> debugLog = null)
```

**Calls ->**
- [[MfaTimingMerger.Align]]
- [[MfaTimingMerger.ApplySentenceTimings]]
- [[MfaTimingMerger.ApplyWordTimings]]
- [[MfaTimingMerger.BuildBookTimingMap]]
- [[MfaTimingMerger.BuildBookTokens]]
- [[MfaTimingMerger.BuildTimedTgTokens]]

**Called-by <-**
- [[MergeTimingsCommand.ExecuteAsync]]
- [[PickupMfaRefinementService.AlignMfaWordsToAsrTokens]]
- [[MfaTimingMergerTests.MergeAndApply_AssignsTimingsToQuoteWrappedBoundaryWords]]
- [[MfaTimingMergerTests.MergeAndApply_NormalizesNumericBookTokensConsistentlyWithMfaCorpus]]

