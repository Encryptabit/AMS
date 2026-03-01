---
namespace: "Ams.Core.Runtime.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# DocumentSlot<T>::Invalidate
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs`

## Summary
**Invalidates the cached slot value so it will reload on next access, optionally keeping dirty status.**

`Invalidate` clears the slot’s loaded cache state by setting `_loaded = false` and `_value = null`. It conditionally preserves dirty state when `keepDirty` is true; otherwise it resets `_dirty = false`. This enables forced reload semantics with optional retention of pending-change intent.


#### [[DocumentSlot_T_.Invalidate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Invalidate(bool keepDirty = false)
```

**Called-by <-**
- [[ChapterDocuments.InvalidateTextGrid]]

