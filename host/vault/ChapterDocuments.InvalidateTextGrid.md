---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs"
access_modifier: "internal"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# ChapterDocuments::InvalidateTextGrid
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs`

## Summary
**Invalidates the cached text-grid document slot so it will be reloaded when needed.**

`InvalidateTextGrid` is a narrow cache-control helper that delegates directly to `_textGrid.Invalidate()`. It does not mutate other slots, perform IO, or apply validation logic. The method exists to force lazy reload/re-evaluation of the text-grid document slot on next access.


#### [[ChapterDocuments.InvalidateTextGrid]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal void InvalidateTextGrid()
```

**Calls ->**
- [[DocumentSlot_T_.Invalidate]]

**Called-by <-**
- [[RunMfaCommand.ExecuteAsync]]

