---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "internal"
base_class: ~
interfaces: []
member_count: 17
dependency_count: 1
pattern: ~
tags:
  - class
---

# ValidateTimingSession

> Class in `Ams.Cli.Commands`

**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Dependencies
- [[IWorkspace]] (`workspace`)

## Properties
- `StructuralEpsilon`: double
- `_workspace`: IWorkspace
- `_transcriptFile`: FileInfo
- `_bookIndexFile`: FileInfo
- `_hydrateFile`: FileInfo
- `_runProsodyAnalysis`: bool
- `_includeAllIntraSentenceGaps`: bool
- `_interSentenceOnly`: bool
- `_policy`: PausePolicy
- `_policySourcePath`: string?
- `_pauseAdjustmentsFile`: FileInfo
- `_prosodyAnalysis`: PauseAnalysisReport?

## Members
- [[ValidateTimingSession..ctor]]
- [[ValidateTimingSession.RunAsync]]
- [[ValidateTimingSession.RunHeadlessAsync]]
- [[ValidateTimingSession.LoadSessionContextAsync]]
- [[ValidateTimingSession.TryLoadMfaSilences]]
- [[ValidateTimingSession.BuildTextGridCandidates]]
- [[ValidateTimingSession.IsSilenceLabel]]
- [[ValidateTimingSession.ExtractBookText]]
- [[ValidateTimingSession.BuildSentenceLookup]]
- [[ValidateTimingSession.BuildParagraphData]]
- [[ValidateTimingSession.RenderIntro]]
- [[ValidateTimingSession.OnCommit]]
- [[ValidateTimingSession.PersistPauseAdjustments]]
- [[ValidateTimingSession.BuildAdjustmentsIncludingStatic]]
- [[ValidateTimingSession.BuildStaticBufferAdjustments]]
- [[ValidateTimingSession.IsStructuralClass]]
- [[ValidateTimingSession.GetRelativePathSafe]]

