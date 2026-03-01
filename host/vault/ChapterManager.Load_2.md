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
# ChapterManager::Load
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Loads a chapter context by chapter ID while updating cursor state and reusing or creating cached context data.**

`Load(string chapterId)` resolves a chapter context by ID via a case-insensitive linear search over `_descriptors`. It validates input with `ArgumentException.ThrowIfNullOrEmpty`, updates `_cursor` to the matched descriptor index, and returns `GetOrCreate(_descriptors[i])` for cache-backed context reuse/creation. If no match is found, it throws `KeyNotFoundException` including both chapter and book identifiers for diagnostics.


#### [[ChapterManager.Load_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterContext Load(string chapterId)
```

**Calls ->**
- [[ChapterManager.GetOrCreate]]

**Called-by <-**
- [[ChapterManager.CreateContext]]

