---
namespace: "Ams.Cli.Services"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Services/PlugalyzerService.cs"
access_modifier: "internal"
complexity: 3
fan_in: 3
fan_out: 2
tags:
  - method
---
# PlugalyzerService::RunAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Services/PlugalyzerService.cs`


#### [[PlugalyzerService.RunAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static Task<int> RunAsync(IReadOnlyList<string> arguments, string workingDirectory, CancellationToken cancellationToken, Action<string> onStdOut = null, Action<string> onStdErr = null)
```

**Calls ->**
- [[PlugalyzerService.ResolveExecutable]]
- [[Log.Debug]]

**Called-by <-**
- [[DspCommand.CreateInitCommand]]
- [[DspCommand.CreateListParamsCommand]]
- [[DspCommand.RunChainAsync]]

