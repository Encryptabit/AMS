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
# FileArtifactResolver::LoadAnchors
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Loads the chapter anchor document from its canonical JSON artifact file.**

`LoadAnchors` is a thin delegation method that resolves the chapter anchor artifact path (`GetChapterArtifactPath(context, "align.anchors.json")`) and loads it through `LoadJson<AnchorDocument>`. It performs no local validation, transformation, or fallback logic. Any null-return or exception behavior is inherited from the shared `LoadJson` implementation.


#### [[FileArtifactResolver.LoadAnchors]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AnchorDocument LoadAnchors(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]
- [[FileArtifactResolver.LoadJson]]

