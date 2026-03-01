---
namespace: "Ams.Core.Runtime.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs"
access_modifier: "public"
complexity: 3
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/di
---
# DocumentSlot<T>::Save
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs`

## Summary
**Commits pending slot changes through the configured saver delegate and resets dirty state.**

`Save` persists slot state only when `_dirty` is true. It short-circuits when clean, and if dirty-but-null (`_value is null`) it clears `_dirty` without calling the saver. Otherwise it invokes `_saver(_value)` and marks the slot clean. Persistence mechanics and any exceptions are delegated to the injected saver callback.


#### [[DocumentSlot_T_.Save]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Save()
```

**Called-by <-**
- [[BookDocuments.SaveChanges]]
- [[ChapterDocuments.SaveChanges]]

