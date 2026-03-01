---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ChapterManager::DetermineChapterStem
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Computes the chapter stem from an explicit identifier or artifact filenames, failing when no identifier source is available.**

`DetermineChapterStem` resolves a chapter stem using a strict fallback chain. It returns `supplied` when non-blank, otherwise uses the first available file (`audioFile ?? asrFile`) and derives a stem by splitting `candidate.Name` on `.` and applying `Path.GetFileNameWithoutExtension` to the first segment. If neither source is available, it throws `ArgumentException("Chapter identifier must be provided.")`. This method centralizes chapter-ID derivation and enforces a required identifier.


#### [[ChapterManager.DetermineChapterStem]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string DetermineChapterStem(string supplied, FileInfo audioFile, FileInfo asrFile)
```

**Called-by <-**
- [[ChapterManager.CreateContext]]

