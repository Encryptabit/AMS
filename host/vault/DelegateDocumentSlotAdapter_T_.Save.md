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
  - llm/validation
  - llm/error-handling
---
# DelegateDocumentSlotAdapter<T>::Save
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Common/DelegateDocumentSlotAdapter.cs`

## Summary
**Persists a document by null-checking it and invoking the configured saver delegate.**

`Save` validates its input and forwards persistence to the injected saver delegate. It throws on null via `ArgumentNullException.ThrowIfNull(document)` and then executes `_saver(document)` without additional logic. Error behavior beyond null checking is delegated to the provided saver implementation.


#### [[DelegateDocumentSlotAdapter_T_.Save]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Save(T document)
```

