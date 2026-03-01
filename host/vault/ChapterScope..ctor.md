---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/di
---
# ChapterScope::.ctor
**Path**: `Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs`

## Summary
**Initialize a chapter-scoped REPL context object with the current state and prior file context.**

This `ChapterScope` constructor is a straight-line initializer (complexity 1) that captures the incoming `ReplState` and `FileInfo previous` into the scope instance for subsequent scope lifecycle operations. Its implementation is non-branching and non-async, indicating it only wires constructor inputs to internal state.


#### [[ChapterScope..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterScope(ReplState state, FileInfo previous)
```

