---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "internal"
complexity: 17
fan_in: 1
fan_out: 15
tags:
  - method
  - danger/high-complexity
  - llm/entry-point
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# MfaWorkflow::RunChapterAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`

> [!danger] High Complexity (17)
> Cyclomatic complexity: 17. Consider refactoring into smaller methods.

## Summary
**It executes the MFA alignment workflow for a chapter, including corpus prep, optional pronunciation augmentation, alignment with fallback behavior, and artifact export.**

`RunChapterAsync` orchestrates end-to-end MFA execution for a chapter: it optionally gates on `MfaProcessSupervisor.EnsureReadyAsync` (non-dedicated mode), validates `audioFile`/`hydrateFile`, prepares `alignment/corpus/mfa` directories, resolves/cleans MFA workspace state, stages audio, and writes the `.lab` corpus via `WriteLabFileAsync`. It builds an `MfaChapterContext` and runs G2P/add-words only when sanitized OOVs exist (`FindOovListFile` + `CreateSanitizedOovList`), enforcing command success through `EnsureSuccess` and tracking whether a custom dictionary was produced. It then runs `AlignAsync` with a retry path that catches `InvalidOperationException`, switches from hydrate-derived text to ASR corpus text, and reattempts alignment, followed by copying resulting artifacts (`.TextGrid`, `.g2p.txt`, cleaned OOV list, dictionary zip, analysis CSV) into chapter storage.


#### [[MfaWorkflow.RunChapterAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static Task RunChapterAsync(ChapterContext chapterContext, FileInfo audioFile, FileInfo hydrateFile, string chapterStem, DirectoryInfo chapterDirectory, CancellationToken cancellationToken, bool useDedicatedProcess = false, string workspaceRoot = null)
```

**Calls ->**
- [[MfaService.AddWordsAsync]]
- [[MfaService.AlignAsync]]
- [[MfaService.GeneratePronunciationsAsync]]
- [[MfaWorkflow.CleanupMfaArtifacts]]
- [[MfaWorkflow.CopyIfExists]]
- [[MfaWorkflow.CreateSanitizedOovList]]
- [[MfaWorkflow.EnsureDirectory]]
- [[MfaWorkflow.EnsureSuccess]]
- [[MfaWorkflow.FindOovListFile]]
- [[MfaWorkflow.ResolveMfaRoot]]
- [[MfaWorkflow.StageAudio]]
- [[MfaWorkflow.WriteLabFileAsync]]
- [[MfaProcessSupervisor.EnsureReadyAsync]]
- [[Log.Debug]]
- [[Log.Warn]]

**Called-by <-**
- [[RunMfaCommand.ExecuteAsync]]

