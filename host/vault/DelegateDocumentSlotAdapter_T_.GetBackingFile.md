---
namespace: "Ams.Core.Runtime.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Common/DelegateDocumentSlotAdapter.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
---
# DelegateDocumentSlotAdapter<T>::GetBackingFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Common/DelegateDocumentSlotAdapter.cs`

## Summary
**Retrieves the adapter’s backing file handle when a backing-file accessor delegate is configured.**

`GetBackingFile` returns the optional backing file reference exposed by the adapter’s injected accessor delegate. It uses null-conditional invocation (`_backingFileAccessor?.Invoke()`), so when no accessor is configured it returns `null` instead of throwing. The method performs no filesystem IO or path computation itself.


#### [[DelegateDocumentSlotAdapter_T_.GetBackingFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo GetBackingFile()
```

