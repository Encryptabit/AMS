---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/data-access
  - llm/factory
  - llm/error-handling
---
# FileArtifactResolver::LoadTextGrid
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Loads and parses a chapter TextGrid file into an in-memory `TextGridDocument` when the source file exists.**

`LoadTextGrid` resolves the chapter TextGrid source path via `GetTextGridPath(context)` and returns `null` when the file is absent. When present, it parses word intervals using `TextGridParser.ParseWordIntervals(path).ToList()` and wraps them in a new `TextGridDocument` with the resolved path and `DateTime.UtcNow` as load timestamp. The method does no caching or persistence updates and relies on parser behavior for content validation.


#### [[FileArtifactResolver.LoadTextGrid]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public TextGridDocument LoadTextGrid(ChapterContext context)
```

**Calls ->**
- [[TextGridParser.ParseWordIntervals]]
- [[FileArtifactResolver.GetTextGridPath]]

