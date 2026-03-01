---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs"
access_modifier: "public"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ChapterContext::ResolveArtifactFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs`

## Summary
**Resolves a chapter artifact `FileInfo` from a validated suffix via the configured artifact resolver.**

`ResolveArtifactFile` validates and normalizes a chapter artifact suffix before delegating to the resolver. It rejects null/whitespace input and suffixes that become empty after trimming/removing leading dots, throwing `ArgumentException` in both cases. For valid input, it calls `_resolver.GetChapterArtifactFile(this, trimmedSuffix)` and returns the resulting `FileInfo`. This centralizes chapter-scoped artifact path resolution with basic suffix sanitization.


#### [[ChapterContext.ResolveArtifactFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo ResolveArtifactFile(string suffix)
```

**Calls ->**
- [[IArtifactResolver.GetChapterArtifactFile]]

**Called-by <-**
- [[TreatCommand.Create]]
- [[PipelineService.RunChapterAsync]]

