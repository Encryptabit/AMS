---
namespace: "Ams.Cli.Models"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Models/FilterChainConfig.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# FilterChainConfig::LoadAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Models/FilterChainConfig.cs`


#### [[FilterChainConfig.LoadAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<FilterChainConfig> LoadAsync(FileInfo path, CancellationToken cancellationToken)
```

**Called-by <-**
- [[DspCommand.CreateFilterChainRunCommand]]

