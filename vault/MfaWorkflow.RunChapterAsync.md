---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "internal"
complexity: 17
fan_in: 1
fan_out: 15
tags:
  - method
  - danger/high-complexity
---
# MfaWorkflow::RunChapterAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`

> [!danger] High Complexity (17)
> Cyclomatic complexity: 17. Consider refactoring into smaller methods.


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

