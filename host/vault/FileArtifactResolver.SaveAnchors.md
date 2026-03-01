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
# FileArtifactResolver::SaveAnchors
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Saves a chapter anchor document to its canonical JSON artifact location.**

`SaveAnchors` is a pass-through persistence method that writes an `AnchorDocument` to the standard chapter anchors artifact path. It computes the target file with `GetChapterArtifactPath(context, "align.anchors.json")` and delegates serialization/write semantics to `SaveJson`. The method contains no local validation or transformation logic.


#### [[FileArtifactResolver.SaveAnchors]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SaveAnchors(ChapterContext context, AnchorDocument document)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]
- [[FileArtifactResolver.SaveJson]]

