---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# ChapterManager::MergeDescriptors
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Merges two chapter descriptors into a single normalized descriptor using incoming-over-existing precedence with alias union.**

`MergeDescriptors` combines an existing and incoming `ChapterDescriptor` with explicit precedence rules. It unions aliases case-insensitively (`HashSet`), prefers incoming root path only when non-blank, prefers incoming audio buffers only when non-empty, and uses null-coalescing for documents/book-word bounds (`incoming` overrides when present). It preserves `existing.ChapterId` and returns a newly constructed `ChapterDescriptor` carrying merged values.


#### [[ChapterManager.MergeDescriptors]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ChapterDescriptor MergeDescriptors(ChapterDescriptor existing, ChapterDescriptor incoming)
```

**Called-by <-**
- [[ChapterManager.UpsertDescriptor]]

