---
namespace: "Ams.Cli.Workspace"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Workspace/CliWorkspace.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
---
# CliWorkspace::.ctor
**Path**: `Projects/AMS/host/Ams.Cli/Workspace/CliWorkspace.cs`

## Summary
**Initialize a `CliWorkspace` with the current REPL state and an optional artifact resolver dependency.**

This constructor wires up `CliWorkspace` with a required `ReplState` and an optional `IArtifactResolver` (defaulting to `null`). With cyclomatic complexity 1, the implementation is straight-line initialization, typically assigning/storing these dependencies without branching or loops.


#### [[CliWorkspace..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public CliWorkspace(ReplState state, IArtifactResolver resolver = null)
```

