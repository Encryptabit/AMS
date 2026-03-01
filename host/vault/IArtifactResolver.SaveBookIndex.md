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
---
# IArtifactResolver::SaveBookIndex
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Declares the resolver API for persisting a book index artifact for a book context.**

`SaveBookIndex` is an `IArtifactResolver` interface contract member, so it specifies behavior but provides no implementation. It defines the write-side operation for persisting a `BookIndex` for a given `BookContext`, complementing `LoadBookIndex`. Validation rules, storage medium, and failure semantics are implementation-defined (e.g., file-based resolver vs. alternate backend).


#### [[IArtifactResolver.SaveBookIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void SaveBookIndex(BookContext context, BookIndex bookIndex)
```

**Called-by <-**
- [[BookDocuments..ctor]]

