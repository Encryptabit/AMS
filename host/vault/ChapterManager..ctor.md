---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "internal"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/validation
  - llm/error-handling
---
# ChapterManager::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Creates a chapter manager by wiring book context dependencies, chapter descriptor state, cache structures, and initial cursor/capacity settings.**

The constructor initializes `ChapterManager` state from a parent `BookContext` and optional cache-capacity hint. It null-checks `bookContext`, snapshots chapter descriptors into `_descriptors`, and sets up case-insensitive caches plus LRU-tracking structures (`_cache`, `_usageNodes`, `_usageOrder`). It stores `maxCachedContexts` without clamping (effective behavior is handled elsewhere) and resets navigation to `_cursor = 0`.


#### [[ChapterManager..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal ChapterManager(BookContext bookContext, int maxCachedContexts = 2147483647)
```

