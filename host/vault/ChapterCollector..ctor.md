---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/di
---
# ChapterCollector::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Creates a chapter collector with a fixed ordered paragraph ID sequence used for later timeline assembly.**

The constructor initializes chapter ordering by materializing the incoming `IEnumerable<int> paragraphIds` into a concrete list (`_orderedParagraphIds = paragraphIds.ToList()`). Internal pause and duration dictionaries are already field-initialized, so no additional setup occurs here. It does not validate IDs or deduplicate; ordering and duplicates are preserved from the input enumeration.


#### [[ChapterCollector..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterCollector(IEnumerable<int> paragraphIds)
```

