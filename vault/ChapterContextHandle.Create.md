---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 2
tags:
  - method
---
# ChapterContextHandle::Create
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs`


#### [[ChapterContextHandle.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static ChapterContextHandle Create(FileInfo bookIndexFile, FileInfo asrFile = null, FileInfo transcriptFile = null, FileInfo hydrateFile = null, FileInfo audioFile = null, DirectoryInfo chapterDirectory = null, string chapterId = null)
```

**Calls ->**
- [[ChapterContextHandle.GetOrCreateManager]]
- [[ChapterManager.CreateContext]]

