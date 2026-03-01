---
namespace: "Ams.Core.Runtime.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
---
# DocumentSlot<T>::SetValue
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs`

## Summary
**Sets the slot value using default dirty-marking semantics by forwarding to the full `SetValue` overload.**

`SetValue(T? value)` is a convenience overload that delegates directly to `SetValue(value, markClean: false)`. It adds no additional logic beyond forwarding arguments, so dirty-state and write-through behavior are determined entirely by the two-parameter overload.


#### [[DocumentSlot_T_.SetValue]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SetValue(T value)
```

**Calls ->**
- [[DocumentSlot_T_.SetValue_2]]

