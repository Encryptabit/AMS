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
  - llm/utility
  - llm/di
---
# IArtifactResolver::SaveTextGrid
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Provide a resolver-level save hook for `TextGridDocument` that intentionally performs no persistence because the source TextGrid file is authoritative.**

`SaveTextGrid` is defined on `IArtifactResolver` but implemented in `FileArtifactResolver` as an intentional no-op. The method only discards `context` and `document` (`_ = context; _ = document;`) and documents that TextGrid content is derived from MFA output and should not be independently persisted. It is still passed as the save delegate in `ChapterDocuments` constructor to satisfy the `DocumentSlot` contract without writing a separate artifact.


#### [[IArtifactResolver.SaveTextGrid]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void SaveTextGrid(ChapterContext context, TextGridDocument document)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]

