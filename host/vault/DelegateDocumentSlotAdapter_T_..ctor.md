---
namespace: "Ams.Core.Runtime.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Common/DelegateDocumentSlotAdapter.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/validation
  - llm/error-handling
---
# DelegateDocumentSlotAdapter<T>::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Common/DelegateDocumentSlotAdapter.cs`

## Summary
**Initializes a generic document-slot adapter with injected load/save delegates and an optional backing-file accessor.**

The constructor wires delegate-backed behaviors for `IDocumentSlotAdapter<T>` by capturing `loader`, `saver`, and optional `backingFileAccessor` into readonly fields. It enforces required delegates with null guards (`loader ?? throw ...`, `saver ?? throw ...`) while allowing a nullable backing-file accessor for adapters that do not expose a backing file. This establishes the callable endpoints later used by `Load`, `Save`, and `GetBackingFile`.


#### [[DelegateDocumentSlotAdapter_T_..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public DelegateDocumentSlotAdapter(Func<T> loader, Action<T> saver, Func<FileInfo> backingFileAccessor = null)
```

