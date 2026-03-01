---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs"
access_modifier: "internal"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/validation
  - llm/error-handling
---
# ChapterContextHandle::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs`

## Summary
**Initializes a chapter context handle with non-null book and chapter context references.**

The `ChapterContextHandle` constructor stores required `BookContext` and `ChapterContext` references for lifecycle management. It validates both parameters with `ArgumentNullException` guards and assigns them to private readonly fields (`_bookContext`, `_chapterContext`). No additional initialization logic is performed beyond dependency capture.


#### [[ChapterContextHandle..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal ChapterContextHandle(BookContext bookContext, ChapterContext chapterContext)
```

