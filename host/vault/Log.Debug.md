---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 1
fan_in: 114
fan_out: 0
tags:
  - method
  - danger/high-fan-in
  - llm/utility
---
# Log::Debug
**Path**: `Projects/AMS/host/Ams.Core/Common/Log.cs`

> [!danger] High Fan-In (114)
> This method is called by 114 other methods. Changes here have wide impact.

## Summary
**Emits a debug-level log entry through the `Log` facade using a message template and variadic arguments.**

`Debug` is an expression-bodied pass-through that forwards directly to the shared static logger via `logger.LogDebug(message, args)`. It introduces no branching, filtering, or argument checks, so message-template formatting and parameter handling are delegated entirely to `Microsoft.Extensions.Logging`.


#### [[Log.Debug]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Debug(string message, params object[] args)
```

**Called-by <-**
- [[AlignCommand.CopyIfRequested]]
- [[AsrCommand.Create]]
- [[BookCommand.PopulatePhonemesAsync]]
- [[BookCommand.RunVerifyAsync]]
- [[BuildIndexCommand.BuildBookIndexAsync]]
- [[BuildIndexCommand.EnsurePhonemesAsync]]
- [[BuildIndexCommand.ProcessBookFromScratch]]
- [[DspCommand.CreateChainAddCommand]]
- [[DspCommand.CreateChainInsertCommand]]
- [[DspCommand.CreateChainPrependCommand]]
- [[DspCommand.CreateChainRemoveCommand]]
- [[DspCommand.CreateInitCommand]]
- [[DspCommand.CreateRunCommand]]
- [[DspCommand.CreateSetDirAddCommand]]
- [[DspCommand.CreateSetDirClearCommand]]
- [[DspCommand.CreateSetDirRemoveCommand]]
- [[DspCommand.ResolveOutputFile]]
- [[DspCommand.RunChainAsync]]
- [[DspCommand.TryDeleteDirectory]]
- [[DspCommand.TryDeleteFile]]
- [[PipelineCommand.ComputeChapterStats]]
- [[PipelineCommand.CreatePrepRenameCommand]]
- [[PipelineCommand.CreatePrepResetCommand]]
- [[PipelineCommand.CreatePrepStageCommand]]
- [[PipelineCommand.CreateRun]]
- [[PipelineCommand.CreateStatsCommand]]
- [[PipelineCommand.CreateVerifyCommand]]
- [[PipelineCommand.LoadMfaSilences]]
- [[PipelineCommand.LogStageInfo]]
- [[PipelineCommand.PerformHardReset]]
- [[PipelineCommand.PerformSoftReset]]
- [[PipelineCommand.ResolveChapterDirectories]]
- [[PipelineCommand.ResolveVerifyTargets]]
- [[PipelineCommand.RunPipelineForMultipleChaptersAsync]]
- [[PipelineCommand.RunStats]]
- [[PipelineCommand.RunVerify]]
- [[RefineSentencesCommand.RunAsync]]
- [[TextCommand.Create]]
- [[TreatCommand.Create]]
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateCommand.CreateServeCommand]]
- [[ValidateCommand.CreateTimingCommand]]
- [[ValidateCommand.CreateTimingInitCommand]]
- [[ValidateCommand.VetPauseAdjustments]]
- [[ValidateTimingSession..ctor]]
- [[ValidateTimingSession.RunHeadlessAsync]]
- [[Program.ExecuteChaptersInParallelAsync]]
- [[Program.Main]]
- [[PlugalyzerService.RunAsync]]
- [[PausePolicyResolver.Resolve]]
- [[GenerateTranscriptCommand.PersistResponse]]
- [[GenerateTranscriptCommand.RunNemoAsync]]
- [[GenerateTranscriptCommand.RunWhisperAsync]]
- [[GenerateTranscriptCommand.TryDelete]]
- [[MergeTimingsCommand.ExecuteAsync]]
- [[MfaDetachedProcessRunner.RunAsync]]
- [[MfaPronunciationProvider.GetPronunciationsAsync]]
- [[MfaPronunciationProvider.RunG2pWithProgressAsync]]
- [[MfaWorkflow.CleanupMfaArtifacts]]
- [[MfaWorkflow.CopyIfExists]]
- [[MfaWorkflow.CreateSanitizedOovList]]
- [[MfaWorkflow.EnsureSuccess]]
- [[MfaWorkflow.FindOovListFile]]
- [[MfaWorkflow.RunChapterAsync]]
- [[MfaWorkflow.TryDeleteDirectory]]
- [[MfaWorkflow.WriteLabFileAsync]]
- [[PronunciationLexiconCache.ReadCoreAsync]]
- [[AsrProcessSupervisor.CreateStartInfoForScript]]
- [[AsrProcessSupervisor.EnsureServiceReadyAsync]]
- [[AsrProcessSupervisor.KillProcess]]
- [[AsrProcessSupervisor.Shutdown]]
- [[AsrProcessSupervisor.StartManagedProcess]]
- [[AsrProcessSupervisor.TriggerBackgroundWarmup]]
- [[MfaProcessSupervisor.PumpAsync]]
- [[MfaProcessSupervisor.ResolveWorkspaceRootForSupervisor]]
- [[MfaProcessSupervisor.RunAsync]]
- [[MfaProcessSupervisor.Shutdown]]
- [[MfaProcessSupervisor.StartProcessAsync]]
- [[MfaProcessSupervisor.TearDownProcess]]
- [[MfaProcessSupervisor.TriggerBackgroundWarmup]]
- [[MfaProcessSupervisor.WaitForReadyAsync]]
- [[AsrEngineConfig.DownloadModelIfMissingAsync]]
- [[AudioTreatmentService.PrepareRoomtoneSegment]]
- [[AudioTreatmentService.TreatChapterCoreAsync]]
- [[AsrProcessor.RunWhisperPassAsync]]
- [[PauseDynamicsService.BuildInterSentenceSpans]]
- [[PauseDynamicsService.BuildIntraSentenceSpans]]
- [[PauseDynamicsService.ExtractTimesFromScript]]
- [[PauseDynamicsService.MatchSilencesToPunctuation]]
- [[PauseDynamicsService.PlanTransforms]]
- [[AudioBufferContext.Unload]]
- [[AudioBufferManager.Deallocate]]
- [[AudioBufferManager.DeallocateAll]]
- [[AudioBufferManager.GetOrCreate]]
- [[BookAudio.LoadRoomtone]]
- [[BookAudio.UnloadRoomtone]]
- [[BookManager.Deallocate]]
- [[BookManager.DeallocateAll]]
- [[BookManager.GetOrCreate]]
- [[BookPhonemePopulator.PopulateMissingAsync]]
- [[ChapterManager.Deallocate]]
- [[ChapterManager.EnsureCapacity]]
- [[ChapterManager.GetOrCreate]]
- [[TranscriptHydrationService.BuildPronunciationFallbackAsync]]
- [[PipelineService.BuildBookIndexAsync]]
- [[PipelineService.EnsurePhonemesAsync]]
- [[PickupMatchingService.SegmentUtterances]]
- [[PickupMatchingService.WriteNamedAsrCache]]
- [[PickupMatchingService.WriteNamedMfaCache]]
- [[PickupMfaRefinementService.AlignMfaWordsToAsrTokens]]
- [[PickupMfaRefinementService.LogMfaResult]]
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]
- [[PickupMfaRefinementService.WriteAsrResponseCache]]
- [[PolishService.WriteMatchedArtifacts]]

