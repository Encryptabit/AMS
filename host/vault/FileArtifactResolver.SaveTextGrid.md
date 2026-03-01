---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
---
# FileArtifactResolver::SaveTextGrid
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Implements a deliberate no-op save path for TextGrid artifacts because they are treated as externally derived source files.**

`SaveTextGrid` is intentionally a no-op: it discards both parameters (`_ = context; _ = document;`) and performs no file IO. The inline comment explains the design decision that `TextGridDocument` is derived from MFA output TextGrid files and therefore does not require separate persistence. This method exists to satisfy the artifact resolver contract without duplicating source artifacts.


#### [[FileArtifactResolver.SaveTextGrid]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SaveTextGrid(ChapterContext context, TextGridDocument document)
```

