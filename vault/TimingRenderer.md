---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
base_class: ~
interfaces:
  - "System.IDisposable"
member_count: 21
dependency_count: 3
pattern: ~
tags:
  - class
---

# TimingRenderer

> Class in `Ams.Cli.Commands`

**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

**Implements**:
- IDisposable

## Dependencies
- [[InteractiveState]] (`state`)
- [[Ams.Core.Prosody.PauseAnalysisReport_]] (`analysisSummary`)
- [[PausePolicy]] (`policy`)

## Properties
- `_state`: InteractiveState
- `_analysisSummary`: PauseAnalysisReport?
- `_policy`: PausePolicy

## Members
- [[TimingRenderer..ctor]]
- [[TimingRenderer.Render]]
- [[TimingRenderer.BuildLayout]]
- [[TimingRenderer.SoftClearViewport]]
- [[TimingRenderer.BuildTree]]
- [[TimingRenderer.FormatTreeLabel]]
- [[TimingRenderer.BuildTreeSummary]]
- [[TimingRenderer.BuildDetailAnalytics]]
- [[TimingRenderer.BuildChapterDetail]]
- [[TimingRenderer.BuildParagraphDetail]]
- [[TimingRenderer.BuildSentenceDetail]]
- [[TimingRenderer.BuildOptionsPanel]]
- [[TimingRenderer.BuildPauseDetail]]
- [[TimingRenderer.BuildManuscript]]
- [[TimingRenderer.WrapInPanel]]
- [[TimingRenderer.CreateStatsTable]]
- [[TimingRenderer.EnumerateStats]]
- [[TimingRenderer.CreateClassTable]]
- [[TimingRenderer.DescribePolicyWindow]]
- [[TimingRenderer.BuildDiffTable]]
- [[TimingRenderer.Dispose]]

