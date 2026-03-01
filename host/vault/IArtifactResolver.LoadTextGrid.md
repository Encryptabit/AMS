---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/di
  - llm/utility
---
# IArtifactResolver::LoadTextGrid
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Load a chapter’s TextGrid alignment file into a `TextGridDocument` by parsing interval data from disk.**

`LoadTextGrid` in `IArtifactResolver` is implemented by `FileArtifactResolver` as a file-backed loader over the chapter TextGrid artifact path. It resolves the path via `GetTextGridPath(context)`, returns `null` if the file is missing, otherwise parses word intervals with `TextGridParser.ParseWordIntervals(path).ToList()` and constructs `new TextGridDocument(path, DateTime.UtcNow, intervals)`. `ChapterDocuments` wires this method in its constructor as the `DocumentSlot<TextGridDocument>` load delegate.


#### [[IArtifactResolver.LoadTextGrid]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
TextGridDocument LoadTextGrid(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]

