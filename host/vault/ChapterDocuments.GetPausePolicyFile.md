---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs"
access_modifier: "internal"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
---
# ChapterDocuments::GetPausePolicyFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs`

## Summary
**Returns the backing file handle for the pause-policy document slot.**

`GetPausePolicyFile` is an internal pass-through accessor that returns `_pausePolicy.GetBackingFile()`. It contains no local validation, path computation, or IO. In implementation the return is nullable (`FileInfo?`), reflecting that a backing file may not exist.


#### [[ChapterDocuments.GetPausePolicyFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal FileInfo GetPausePolicyFile()
```

**Calls ->**
- [[DocumentSlot_T_.GetBackingFile]]

