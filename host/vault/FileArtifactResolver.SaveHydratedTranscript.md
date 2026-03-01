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
# FileArtifactResolver::SaveHydratedTranscript
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Writes a chapter’s hydrated transcript to its standard JSON artifact file.**

`SaveHydratedTranscript` delegates hydrated transcript persistence to the resolver’s generic JSON writer. It builds the canonical artifact path with `GetChapterArtifactPath(context, "align.hydrate.json")` and calls `SaveJson` with the provided `hydrated` model. The method contains no local validation or transformation, relying on `SaveJson` for serialization and argument handling.


#### [[FileArtifactResolver.SaveHydratedTranscript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SaveHydratedTranscript(ChapterContext context, HydratedTranscript hydrated)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]
- [[FileArtifactResolver.SaveJson]]

