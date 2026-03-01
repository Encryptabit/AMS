---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/MergeTimingsCommand.cs"
access_modifier: "public"
complexity: 42
fan_in: 1
fan_out: 6
tags:
  - method
  - danger/high-complexity
  - llm/entry-point
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# MergeTimingsCommand::ExecuteAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/MergeTimingsCommand.cs`

> [!danger] High Complexity (42)
> Cyclomatic complexity: 42. Consider refactoring into smaller methods.

## Summary
**It applies TextGrid-derived timing alignment to hydrated words and/or transcript sentences for a chapter, then persists changes when updates are produced.**

`ExecuteAsync` is a synchronous command-style orchestrator (Task-returning) that conditionally merges MFA `TextGrid` timings into chapter artifacts based on `MergeTimingsOptions` flags (`ApplyToHydrate`/`ApplyToTranscript`). It resolves and parses word intervals from `options.TextGridFile` or `chapter.Documents.GetTextGridFile()`, validates prerequisites (usable intervals, loaded `BookIndex`, valid chapter word window from `ResolveChapterWordWindow`), builds `WordTarget`/`SentenceTarget` update callbacks over copied hydrate/transcript collections, and runs `MfaTimingMerger.MergeAndApply(...)` with a debug callback. It writes back updated immutable records to `chapter.Documents`, calls `chapter.Documents.SaveChanges()` only when mutations occurred, emits a detailed merge report, and exits via `Task.CompletedTask`.


#### [[MergeTimingsCommand.ExecuteAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task ExecuteAsync(ChapterContext chapter, MergeTimingsOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[MergeTimingsCommand.ResolveChapterWordWindow]]
- [[Log.Debug]]
- [[MfaTimingMerger.MergeAndApply]]
- [[TextGridParser.ParseWordIntervals]]
- [[ChapterDocuments.GetTextGridFile]]
- [[ChapterDocuments.SaveChanges]]

**Called-by <-**
- [[PipelineService.RunChapterAsync]]

