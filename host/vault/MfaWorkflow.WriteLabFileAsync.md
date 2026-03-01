---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# MfaWorkflow::WriteLabFileAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`

## Summary
**It generates the chapter LAB file for MFA alignment from ASR corpus text when possible, otherwise from hydrated transcript text, with validation and fallback handling.**

`WriteLabFileAsync` builds the MFA `.lab` transcript text by preferring `corpusSource` when provided and existing: it reads lines asynchronously, normalizes/filter them through `PrepareLabLines`, logs selection/fallback decisions via `Log.Debug`, and writes UTF-8 output to `labPath` if usable content exists. If corpus-source content is unusable, it falls back to `chapterContext.Documents.HydratedTranscript?.Sentences.Select(s => s.BookText)`, applies the same normalization, and writes when non-empty. If neither source yields valid lines, it throws `InvalidOperationException` naming the chapter.


#### [[MfaWorkflow.WriteLabFileAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task WriteLabFileAsync(FileInfo hydrateFile, ChapterContext chapterContext, string labPath, FileInfo corpusSource, CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaWorkflow.PrepareLabLines]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]

