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
# IArtifactResolver::LoadBookIndex
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Defines the resolver contract for loading a book-level index artifact from a `BookContext`.**

`LoadBookIndex` is an interface member on `IArtifactResolver`, so it defines a contract rather than concrete logic. Its declared signature in code is `BookIndex? LoadBookIndex(BookContext context)`, with nullable return semantics indicating implementations may return no index (for example, when artifacts are absent). It is the read-side counterpart to `SaveBookIndex` in the same resolver abstraction.


#### [[IArtifactResolver.LoadBookIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
BookIndex LoadBookIndex(BookContext context)
```

**Called-by <-**
- [[BookDocuments..ctor]]

