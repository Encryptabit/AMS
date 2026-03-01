---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/di
---
# NodeOptionBundle::.ctor
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**This constructor centralizes creation and command registration of all CLI options needed to define a DSP node.**

The `NodeOptionBundle(Command command)` constructor initializes a full set of `System.CommandLine` options for DSP node configuration, assigning each to a strongly typed property (`Option<string>`, `Option<int?>`, `Option<FileInfo?>`, etc.). It marks `--plugin` as required via `IsRequired = true`, and uses `() => Array.Empty<string>()` defaults for repeatable array options like `--param`, `--input`, and `--arg`. After constructing all option instances (`--name`, `--description`, `--preset`, `--sample-rate`, `--block-size`, `--out-channels`, `--bit-depth`, `--midi-input`, `--output-file`, etc.), it registers them on the supplied `Command` by calling `command.AddOption(...)` for each.


#### [[NodeOptionBundle..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public NodeOptionBundle(Command command)
```

