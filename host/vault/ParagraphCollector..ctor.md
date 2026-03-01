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
# ParagraphCollector::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Initializes a paragraph collector with its paragraph ID and associated sentence ID list.**

The constructor performs minimal object initialization by assigning the incoming `paragraphId` and `sentenceIds` to `ParagraphId` and `SentenceIds`. Its internal dictionaries are field-initialized, so construction is O(1) and does no additional setup work. It also does not validate or copy `sentenceIds`, retaining the provided `IReadOnlyList<int>` reference.


#### [[ParagraphCollector..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ParagraphCollector(int paragraphId, IReadOnlyList<int> sentenceIds)
```

