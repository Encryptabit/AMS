---
namespace: "Ams.Core.Runtime.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# DocumentSlot<T>::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs`

## Summary
**Creates a document slot with delegate-based load/save behavior and optional post-load, backing-file, and write-through configuration.**

The constructor initializes a `DocumentSlot<T>` by capturing injected load/save delegates (`_loader`, `_saver`) and optional behavior from `DocumentSlotOptions<T>`. It enforces non-null delegates with `ArgumentNullException` guards, then conditionally copies `PostLoadTransform`, `BackingFileAccessor`, and `WriteThrough` into private fields when options are provided. It does not perform any load/save work at construction time; lazy state remains defaulted until later calls.


#### [[DocumentSlot_T_..ctor_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public DocumentSlot(Func<T> loader, Action<T> saver, DocumentSlotOptions<T> options = null)
```

