---
namespace: "Ams.Cli.Services"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Services/DspConfigService.cs"
access_modifier: "internal"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# DspConfigService::GetConfigFilePath
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Services/DspConfigService.cs`


#### [[DspConfigService.GetConfigFilePath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static string GetConfigFilePath()
```

**Calls ->**
- [[AmsAppDataPaths.Resolve]]

**Called-by <-**
- [[DspConfigService.LoadAsync]]
- [[DspConfigService.SaveAsync]]

