---
namespace: "Ams.Core.Runtime.Interfaces"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Interfaces/IBookManager.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/entry-point
  - llm/di
  - llm/utility
---
# IBookManager::Load
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Interfaces/IBookManager.cs`

## Summary
**Declares the book manager API for loading a book context by positional index.**

`Load(int index)` is an `IBookManager` interface contract for index-based book context retrieval. As an interface member, it contains no implementation logic and leaves bounds checks, caching, and error behavior to concrete managers. It defines one of the primary navigation/load entry points for book management.


#### [[IBookManager.Load]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
BookContext Load(int index)
```

