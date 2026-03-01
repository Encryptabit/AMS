---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/data-access
  - llm/utility
---
# FileArtifactResolver::SaveAsr
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Saves a chapter ASR response to its standard JSON artifact file.**

`SaveAsr` is a thin persistence wrapper that writes an `AsrResponse` to the chapter ASR artifact path. It computes the target location using `GetChapterArtifactPath(context, "asr.json")` and delegates serialization/write operations to `SaveJson`. The method itself performs no local validation or transformation logic.


#### [[FileArtifactResolver.SaveAsr]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SaveAsr(ChapterContext context, AsrResponse asr)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]
- [[FileArtifactResolver.SaveJson]]

