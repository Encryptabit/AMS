---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/RunMfaCommand.cs"
access_modifier: "public"
complexity: 19
fan_in: 1
fan_out: 5
tags:
  - method
  - danger/high-complexity
  - llm/entry-point
  - llm/data-access
  - llm/async
  - llm/validation
  - llm/error-handling
---
# RunMfaCommand::ExecuteAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/RunMfaCommand.cs`

> [!danger] High Complexity (19)
> Cyclomatic complexity: 19. Consider refactoring into smaller methods.

## Summary
**It orchestrates running MFA alignment for a chapter by resolving required files and returning the TextGrid artifact path while invalidating cached TextGrid state.**

`ExecuteAsync` validates `chapter`, resolves `chapterRoot/chapterStem`, and computes effective inputs (`chapterDirectory`, `hydrateFile`, `audioFile`) from `options` with fallbacks to chapter artifacts via `GetHydratedTranscriptFile()` and `ResolveAudioFile(...)`. It fails fast with `InvalidOperationException` for missing configuration and `FileNotFoundException` when hydrate/audio files are absent, then awaits `MfaWorkflow.RunChapterAsync(...)` with optional dedicated-process/workspace settings. After execution, it resolves the TextGrid output path from `options.TextGridFile`, `chapter.Documents.GetTextGridFile()`, or `<alignmentRoot>/mfa/<chapterStem>.TextGrid`, calls `chapter.Documents.InvalidateTextGrid()`, and returns `new RunMfaResult(textGridFile)`.


#### [[RunMfaCommand.ExecuteAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<RunMfaResult> ExecuteAsync(ChapterContext chapter, RunMfaOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[RunMfaCommand.ResolveAudioFile]]
- [[MfaWorkflow.RunChapterAsync]]
- [[ChapterDocuments.GetHydratedTranscriptFile]]
- [[ChapterDocuments.GetTextGridFile]]
- [[ChapterDocuments.InvalidateTextGrid]]

**Called-by <-**
- [[PipelineService.RunChapterAsync]]

