---
namespace: "Ams.Core.Runtime.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Common/DelegateDocumentSlotAdapter.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/di
---
# DelegateDocumentSlotAdapter<T>::Load
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Common/DelegateDocumentSlotAdapter.cs`

## Summary
**Loads a document instance by invoking the adapter’s configured loader delegate.**

`Load` is a direct delegate invocation wrapper that returns `_loader()` as the adapter’s loaded document value. It performs no local validation, caching, or exception handling; all behavior is determined by the injected loader delegate. The implementation is expression-bodied and O(1) aside from delegate execution cost.


#### [[DelegateDocumentSlotAdapter_T_.Load]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public T Load()
```

