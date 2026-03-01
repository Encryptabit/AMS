---
namespace: "Ams.Core.Runtime.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/di
---
# DocumentSlot<T>::SetValue
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs`

## Summary
**Sets the slot’s current value while controlling dirty-state and optional immediate save semantics.**

`SetValue(T? value, bool markClean)` is the core state-transition method for `DocumentSlot<T>`. It assigns `_value`, marks the slot as loaded, and then either forces a clean state (`markClean`), performs immediate persistence when `_writeThrough` is enabled and `value` is non-null (`_saver(value)`), or sets `_dirty` based on whether a non-null value is present. This method centralizes dirty tracking and optional write-through save behavior.


#### [[DocumentSlot_T_.SetValue_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SetValue(T value, bool markClean)
```

**Called-by <-**
- [[BookDocuments.SetLoadedBookIndex]]
- [[DocumentSlot_T_.SetValue]]

