---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
  - llm/data-access
---
# ValidateCommand::EnsureWritable
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Prepare an output file location and prevent accidental replacement unless overwrite is explicitly enabled.**

`EnsureWritable` is a static guard that prepares the target path by creating the parent directory when `file.DirectoryName` is non-null (`Directory.CreateDirectory(Path.GetFullPath(...))`). It then enforces overwrite policy with `if (file.Exists && !overwrite)` and throws an `IOException` containing a `--overwrite` remediation hint. The method performs no write itself; it validates filesystem preconditions and fails fast.


#### [[ValidateCommand.EnsureWritable]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void EnsureWritable(FileInfo file, bool overwrite)
```

