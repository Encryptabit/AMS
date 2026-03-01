---
namespace: "Ams.Cli.Workspace"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Workspace/CliWorkspace.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/di
  - llm/utility
---
# CliWorkspace::.ctor
**Path**: `Projects/AMS/host/Ams.Cli/Workspace/CliWorkspace.cs`

## Summary
**Instantiate a CLI workspace with optional state/resolver dependencies and build its workspace descriptor from the provided root path.**

This constructor initializes `CliWorkspace` from a root path while accepting nullable `ReplState` and `IArtifactResolver` collaborators for optional dependency injection. Its logic is intentionally shallow (complexity 3), with the key operation delegated to `BuildDescriptor` so descriptor creation stays centralized outside constructor wiring.


#### [[CliWorkspace..ctor_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public CliWorkspace(string rootPath, ReplState state = null, IArtifactResolver resolver = null)
```

**Calls ->**
- [[CliWorkspace.BuildDescriptor]]

