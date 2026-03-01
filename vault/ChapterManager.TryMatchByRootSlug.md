---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
---
# ChapterManager::TryMatchByRootSlug
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.TryMatchByRootSlug]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ChapterDescriptor TryMatchByRootSlug(IReadOnlyList<ChapterDescriptor> descriptors, string normalizedRequested)
```

**Calls ->**
- [[ChapterManager.NormalizeChapterId]]

**Called-by <-**
- [[ChapterManager.EnsureChapterDescriptor]]

