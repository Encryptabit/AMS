---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# ChapterManager::MergeAliases
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Combines and deduplicates chapter alias values from existing and incoming descriptors.**

`MergeAliases` builds a deduplicated alias set by combining IDs and aliases from both descriptors. It initializes a case-insensitive `HashSet<string>`, adds `existing.ChapterId` and `incoming.ChapterId`, then iterates both `Aliases` collections, normalizing/filtering each entry through `AddAlias`. The merged result is materialized as an array and returned as `IReadOnlyCollection<string>`.


#### [[ChapterManager.MergeAliases]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyCollection<string> MergeAliases(ChapterDescriptor existing, ChapterDescriptor incoming)
```

**Calls ->**
- [[ChapterManager.AddAlias]]

**Called-by <-**
- [[ChapterManager.CloneWithAliases]]

