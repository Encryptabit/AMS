---
namespace: "Ams.Core.Runtime.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/di
---
# DocumentSlot<T>::GetValue
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs`

## Summary
**Returns the slot’s current value, loading and transforming it once on first access.**

`GetValue` lazily loads and caches the document value on first access. When `_loaded` is false, it invokes `_loader()`, optionally applies `_postLoadTransform`, stores the result in `_value`, and flips `_loaded` to true; subsequent calls return cached `_value` directly. This method centralizes one-time load semantics with optional post-load normalization.


#### [[DocumentSlot_T_.GetValue]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public T GetValue()
```

