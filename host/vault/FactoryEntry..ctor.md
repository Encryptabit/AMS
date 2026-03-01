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
# FactoryEntry::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Creates a pool entry that stores a specific `WhisperFactory` instance for shared reuse.**

The `FactoryEntry(WhisperFactory factory)` constructor is a minimal initializer that assigns the provided `WhisperFactory` instance to the immutable `Factory` property. It performs no validation, side effects, or reference-count logic itself; `RefCount` remains separately managed by pool acquire/release paths.


#### [[FactoryEntry..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FactoryEntry(WhisperFactory factory)
```

