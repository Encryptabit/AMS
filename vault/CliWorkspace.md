---
namespace: "Ams.Cli.Workspace"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Workspace/CliWorkspace.cs"
access_modifier: "internal"
base_class: ~
interfaces:
  - "Ams.Core.Runtime.Workspace.IWorkspace"
member_count: 6
dependency_count: 3
pattern: ~
tags:
  - class
---

# CliWorkspace

> Class in `Ams.Cli.Workspace`

**Path**: `home/cari/repos/AMS/host/Ams.Cli/Workspace/CliWorkspace.cs`

**Implements**:
- [[IWorkspace]]

## Dependencies
- [[ReplState]] (`state`)
- [[Ams.Core.Runtime.Artifacts.IArtifactResolver_]] (`resolver`)
- [[Ams.Cli.Repl.ReplState_]] (`state`)

## Properties
- `RootPath`: string
- `Book`: BookContext
- `_state`: ReplState?
- `_manager`: BookManager
- `_rootPath`: string

## Members
- [[CliWorkspace..ctor]]
- [[CliWorkspace..ctor_2]]
- [[CliWorkspace.OpenChapter]]
- [[CliWorkspace.NormalizeOptions]]
- [[CliWorkspace.ResolveDefaultBookIndex]]
- [[CliWorkspace.BuildDescriptor]]

