---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "public"
complexity: 2
fan_in: 4
fan_out: 6
tags:
  - method
---
# MfaTimingMerger::MergeAndApply
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`


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

