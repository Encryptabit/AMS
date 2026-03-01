---
namespace: "Ams.Core.Runtime.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# DocumentSlot<T>::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs`

## Summary
**Initializes a document slot using an adapter abstraction instead of raw load/save delegates.**

This overload constructs `DocumentSlot<T>` from an `IDocumentSlotAdapter<T>` by delegating to the delegate-based constructor (`adapter.Load`, `adapter.Save`) and forwarding optional options. It enforces adapter non-null inline in the base-constructor call and stores the adapter in `_adapter` for fallback backing-file access (`GetBackingFile`). The constructor adds no additional state mutation beyond adapter capture.


#### [[DocumentSlot_T_..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public DocumentSlot(IDocumentSlotAdapter<T> adapter, DocumentSlotOptions<T> options = null)
```

