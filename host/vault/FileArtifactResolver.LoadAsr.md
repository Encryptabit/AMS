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
# FileArtifactResolver::LoadAsr
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Loads the chapter ASR response artifact from its standard JSON file.**

`LoadAsr` is a thin forwarding method that resolves the chapter ASR artifact path (`GetChapterArtifactPath(context, "asr.json")`) and delegates deserialization to `LoadJson<AsrResponse>`. It has no local validation, mapping, or fallback logic. Return and error behavior are determined by the underlying shared loader.


#### [[FileArtifactResolver.LoadAsr]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AsrResponse LoadAsr(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]
- [[FileArtifactResolver.LoadJson]]

