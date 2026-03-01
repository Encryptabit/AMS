---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidateCommand::BuildSiblingFile
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Construct a `FileInfo` for a related sibling file path based on a reference file path and a suffix.**

`BuildSiblingFile` is a small path-construction helper that derives a sibling file from `referencePath` by reusing its directory and applying `suffix` to the target filename, then returns a `FileInfo` for that computed path. The implementation is path-string manipulation plus `new FileInfo(...)`, with no direct disk access. With complexity 2, it likely contains a single branch for filename/extension edge handling.


#### [[ValidateCommand.BuildSiblingFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo BuildSiblingFile(string referencePath, string suffix)
```

