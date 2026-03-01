---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/utility
---
# BookIndexer::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Creates a `BookIndexer` with either an injected pronunciation provider or a no-op default provider.**

The constructor initializes the indexer’s pronunciation dependency by assigning `_pronunciationProvider` from the optional parameter. If `pronunciationProvider` is null, it falls back to `NullPronunciationProvider.Instance` via null-coalescing assignment. This implements a null-object default so downstream indexing code can call pronunciation APIs without additional null checks.


#### [[BookIndexer..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookIndexer(IPronunciationProvider pronunciationProvider = null)
```

