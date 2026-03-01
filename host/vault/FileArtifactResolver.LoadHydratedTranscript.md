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
# FileArtifactResolver::LoadHydratedTranscript
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Loads the hydrated transcript artifact for a chapter from its canonical JSON path.**

`LoadHydratedTranscript` is a thin wrapper that resolves the hydrated-transcript artifact location (`GetChapterArtifactPath(context, "align.hydrate.json")`) and delegates deserialization to `LoadJson<HydratedTranscript>`. It does not perform local validation, mapping, or fallback logic. Return semantics (including nullability and error propagation) are inherited from `LoadJson`.


#### [[FileArtifactResolver.LoadHydratedTranscript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public HydratedTranscript LoadHydratedTranscript(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]
- [[FileArtifactResolver.LoadJson]]

