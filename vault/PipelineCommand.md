---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 55
dependency_count: 0
pattern: ~
tags:
  - class
---

# PipelineCommand

> Class in `Ams.Cli.Commands`

**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Properties
- `PipelineStageCount`: int
- `PipelineProgressColumns`: ProgressColumn[]
- `PathComparison`: StringComparison
- `PathComparer`: StringComparer
- `DefaultBatchFolderName`: string
- `CrxDirectoryName`: string
- `StatsJsonOptions`: JsonSerializerOptions
- `VerifyJsonOptions`: JsonSerializerOptions
- `PatternTokenRegex`: Regex
- `StageStyles`: Dictionary<PipelineStage, (string Label, string Color)>
- `UnmatchedTokenRegex`: Regex

## Members
- [[PipelineCommand.LogStageInfo]]
- [[PipelineCommand.Create]]
- [[PipelineCommand.CreatePrepCommand]]
- [[PipelineCommand.RunPipelineForMultipleChaptersAsync]]
- [[PipelineCommand.CreateStatsCommand]]
- [[PipelineCommand.CreateVerifyCommand]]
- [[PipelineCommand.CreatePrepStageCommand]]
- [[PipelineCommand.CreatePrepRenameCommand]]
- [[PipelineCommand.CreatePrepResetCommand]]
- [[PipelineCommand.CreateRun]]
- [[PipelineCommand.RunPipelineAsync]]
- [[PipelineCommand.PerformSoftReset]]
- [[PipelineCommand.PerformHardReset]]
- [[PipelineCommand.RunStats]]
- [[PipelineCommand.ResolveRenameTargets]]
- [[PipelineCommand.ApplyRenamePattern]]
- [[PipelineCommand.ExtractUnmatchedParts]]
- [[PipelineCommand.BuildRenamePlan]]
- [[PipelineCommand.CollectRenameOperations]]
- [[PipelineCommand.ValidateRenamePlans]]
- [[PipelineCommand.ProjectPath]]
- [[PipelineCommand.EnsureTrailingSeparator]]
- [[PipelineCommand.ReplaceStem]]
- [[PipelineCommand.PathsEqual]]
- [[PipelineCommand.ResolveChapterDirectories]]
- [[PipelineCommand.ComputeChapterStats]]
- [[PipelineCommand.PrintStatsReport]]
- [[PipelineCommand.RunVerify]]
- [[PipelineCommand.ResolveVerifyTargets]]
- [[PipelineCommand.ResolveProcessedVariants]]
- [[PipelineCommand.ResolveVariantHydrate]]
- [[PipelineCommand.NormalizeVariantToken]]
- [[PipelineCommand.ToMono]]
- [[PipelineCommand.LoadSentenceTimings]]
- [[PipelineCommand.TryReadTiming]]
- [[PipelineCommand.TryGetDouble]]
- [[PipelineCommand.TryGetInt]]
- [[PipelineCommand.WriteVerificationCsv]]
- [[PipelineCommand.EscapeCsv]]
- [[PipelineCommand.CreateAudioTable]]
- [[PipelineCommand.CreateProsodyTable]]
- [[PipelineCommand.EnumerateStats]]
- [[PipelineCommand.FormatDuration]]
- [[PipelineCommand.FormatDb]]
- [[PipelineCommand.ComputeAudioStats]]
- [[PipelineCommand.LoadMfaSilences]]
- [[PipelineCommand.IsSilenceLabel]]
- [[PipelineCommand.ExtractChapterStem]]
- [[PipelineCommand.LoadJson]]
- [[PipelineCommand.EnsureDirectory]]
- [[PipelineCommand.MakeSafeFileStem]]
- [[PipelineCommand.LooksLikeChapterDirectory]]
- [[PipelineCommand.NormalizeDirectoryPath]]
- [[PipelineCommand.IsWithinDirectory]]
- [[PipelineCommand.GetStagedFileName]]

