---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "private"
base_class: ~
interfaces:
  - "System.IDisposable"
member_count: 2
dependency_count: 1
pattern: ~
tags:
  - class
---

# ChapterScope

> Class in `Ams.Cli.Repl`

**Path**: `Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs`

**Implements**:
- IDisposable

## Dependencies
- [[ReplState]] (`state`)

## Properties
- `_state`: ReplState
- `_previous`: FileInfo?
- `_disposed`: bool

## Members
- [[ChapterScope..ctor]]
- [[ChapterScope.Dispose]]

