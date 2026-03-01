---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
base_class: ~
interfaces: []
member_count: 14
dependency_count: 3
pattern: ~
tags:
  - class
---

# CompressionState

> Class in `Ams.Cli.Commands`

**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Dependencies
- [[ScopeEntry]] (`scope`)
- [[CompressionControls]] (`controls`)
- [[PausePolicy]] (`basePolicy`)

## Properties
- `Scope`: ScopeEntry
- `Controls`: CompressionControls
- `Preview`: List<CompressionPreviewItem>
- `SelectedControlIndex`: int
- `PreviewOffset`: int
- `HasPreview`: bool
- `PausesForCommit`: IReadOnlyList<EditablePause>
- `_pauses`: List<EditablePause>
- `_pauseLookup`: HashSet<EditablePause>

## Members
- [[CompressionState..ctor]]
- [[CompressionState.MatchesScope]]
- [[CompressionState.ResetSelection]]
- [[CompressionState.MoveControlSelection]]
- [[CompressionState.AdjustSelectedControl]]
- [[CompressionState.ScrollPreview]]
- [[CompressionState.GetPreviewSlice]]
- [[CompressionState.GetSnapshot]]
- [[CompressionState.NotifyPauseAdjusted]]
- [[CompressionState.HandleCommit]]
- [[CompressionState.RebuildPreview]]
- [[CompressionState.ApplyPreview]]
- [[CompressionState.IsWithinScope]]
- [[CompressionState.GetPauseParagraphId]]

