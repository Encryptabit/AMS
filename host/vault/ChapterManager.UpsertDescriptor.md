---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ChapterManager::UpsertDescriptor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Adds a new chapter descriptor or merges it into an existing descriptor with the same chapter ID.**

`UpsertDescriptor` inserts or updates a chapter descriptor in `_descriptors` keyed by case-insensitive `ChapterId`. It validates input (`ArgumentNullException.ThrowIfNull`), scans existing descriptors for a matching ID, and on match merges old/new values via `MergeDescriptors` then replaces the list entry and returns the merged descriptor. If no match exists, it appends the new descriptor and returns it. This method centralizes descriptor mutation semantics for caller workflows like `EnsureChapterDescriptor`.


#### [[ChapterManager.UpsertDescriptor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterDescriptor UpsertDescriptor(ChapterDescriptor descriptor)
```

**Calls ->**
- [[ChapterManager.MergeDescriptors]]

**Called-by <-**
- [[ChapterManager.EnsureChapterDescriptor]]

