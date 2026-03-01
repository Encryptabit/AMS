---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/factory
---
# PipelineService::ResolveTextGridFile
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

## Summary
**Generate the default MFA TextGrid file location for a chapter and ensure its parent directory exists.**

`ResolveTextGridFile` deterministically computes a chapter TextGrid path under `chapterRoot/alignment/mfa`, creates that directory with `Directory.CreateDirectory`, and returns a `FileInfo` for `chapterId + ".TextGrid"`. The method is branch-free (complexity 1) and performs no input validation, delegating path/IO failures to the underlying framework. In `RunChapterAsync`, it serves as the fallback resolver when no explicit or pre-existing TextGrid artifact path is available.


#### [[PipelineService.ResolveTextGridFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo ResolveTextGridFile(string chapterRoot, string chapterId)
```

**Called-by <-**
- [[PipelineService.RunChapterAsync]]

