---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
---
# FactoryHandle::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Initializes a disposable handle with the specific pooled factory identity and backing entry it will manage on release.**

The `FactoryHandle(FactoryKey key, FactoryEntry entry)` constructor stores the supplied pool key and entry in private fields (`_key`, `_entry`) used later for lifecycle management. It contains no validation or synchronization; disposal semantics are handled by `Dispose`, which uses these captured references to decrement `RefCount` and potentially remove/dispose the shared factory.


#### [[FactoryHandle..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FactoryHandle(WhisperFactoryPool.FactoryKey key, WhisperFactoryPool.FactoryEntry entry)
```

