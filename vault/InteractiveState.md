---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
base_class: ~
interfaces: []
member_count: 61
dependency_count: 3
pattern: ~
tags:
  - class
---

# InteractiveState

> Class in `Ams.Cli.Commands`

**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Dependencies
- [[ChapterPauseMap]] (`chapter`)
- [[PauseAnalysisReport]] (`analysis`)
- [[PausePolicy]] (`basePolicy`)

## Properties
- `CursorIndex`: int
- `Entries`: IReadOnlyList<ScopeEntry>
- `Current`: ScopeEntry
- `ParagraphCount`: int
- `TotalSentenceCount`: int
- `TotalPauseCount`: int
- `OptionsFocused`: bool
- `LastCommitMessage`: string?
- `HasCompressionPreview`: bool
- `DurationEpsilon`: double
- `_chapter`: ChapterPauseMap
- `_analysis`: PauseAnalysisReport
- `_basePolicy`: PausePolicy
- `_sentenceLookup`: IReadOnlyDictionary<int, string>
- `_paragraphSentences`: IReadOnlyDictionary<int, IReadOnlyList<int>>
- `_sentenceToParagraph`: IReadOnlyDictionary<int, int>
- `_paragraphLookup`: Dictionary<int, ParagraphInfo>
- `_sentencePauses`: Dictionary<int, List<EditablePause>>
- `_chapterPauses`: List<EditablePause>
- `_entries`: List<ScopeEntry>
- `_orderedParagraphIds`: List<int>
- `_baselineTimeline`: Dictionary<int, SentenceTiming>
- `_committedAdjustments`: List<PauseAdjust>
- `_compression`: CompressionState?
- `_treeOffset`: int
- `_treeViewportSize`: int
- `_optionsFocused`: bool
- `_lastCommitMessage`: string?

## Members
- [[InteractiveState..ctor]]
- [[InteractiveState.GetCommittedAdjustments]]
- [[InteractiveState.GetBaselineTimeline]]
- [[InteractiveState.MoveWithinTier]]
- [[InteractiveState.StepInto]]
- [[InteractiveState.StepOut]]
- [[InteractiveState.AdjustCurrent]]
- [[InteractiveState.SetCurrent]]
- [[InteractiveState.ToggleOptionsFocus]]
- [[InteractiveState.UpdateLastCommitMessage]]
- [[InteractiveState.SetTreeViewportSize]]
- [[InteractiveState.GetParagraphInfo]]
- [[InteractiveState.GetSentenceText]]
- [[InteractiveState.IsParagraphZero]]
- [[InteractiveState.FilterParagraphZeroAdjustments]]
- [[InteractiveState.GetParagraphSentenceIds]]
- [[InteractiveState.GetParagraphSentenceCount]]
- [[InteractiveState.CountSentencePauses]]
- [[InteractiveState.CountParagraphPauses]]
- [[InteractiveState.MoveCompressionControlSelection]]
- [[InteractiveState.AdjustCompressionControl]]
- [[InteractiveState.ScrollCompressionPreview]]
- [[InteractiveState.GetCompressionControlsSnapshot]]
- [[InteractiveState.GetCompressionPreview]]
- [[InteractiveState.ApplyCompressionPreview]]
- [[InteractiveState.RefreshCompressionStateIfNeeded]]
- [[InteractiveState.EnsureCompressionStateForCurrentScope]]
- [[InteractiveState.CollectCompressionPauses]]
- [[InteractiveState.GetTreeViewportEntries]]
- [[InteractiveState.GetParagraphRange]]
- [[InteractiveState.GetPendingPauseCount]]
- [[InteractiveState.CommitScope]]
- [[InteractiveState.CollectPauses]]
- [[InteractiveState.GetChapterEntry]]
- [[InteractiveState.EnumerateParagraphEntries]]
- [[InteractiveState.EnumerateSentenceEntries]]
- [[InteractiveState.EnumeratePauseEntriesForSentence]]
- [[InteractiveState.EnumerateChapterPauseEntries]]
- [[InteractiveState.GetPendingAdjustments]]
- [[InteractiveState.EnsureTreeVisibility]]
- [[InteractiveState.DescribePauseContext]]
- [[InteractiveState.TryCreateDiffRow]]
- [[InteractiveState.BuildDiffContext]]
- [[InteractiveState.NotifyCompressionPauseAdjusted]]
- [[InteractiveState.BuildManuscriptMarkup]]
- [[InteractiveState.PopulatePauseLookups]]
- [[InteractiveState.BuildBaselineTimeline]]
- [[InteractiveState.CreateEditablePause]]
- [[InteractiveState.BuildEntries]]
- [[InteractiveState.AppendParagraph]]
- [[InteractiveState.AppendSentence]]
- [[InteractiveState.AppendChapterPause]]
- [[InteractiveState.BuildParagraphLabel]]
- [[InteractiveState.BuildSentenceLabel]]
- [[InteractiveState.BuildPauseLabel]]
- [[InteractiveState.TrimAndEscape]]
- [[InteractiveState.AppendChapterPreview]]
- [[InteractiveState.AppendParagraphMarkup]]
- [[InteractiveState.AppendSentenceFallback]]
- [[InteractiveState.AppendPauseSentencesFallback]]
- [[InteractiveState.MatchesCommittedPause]]

