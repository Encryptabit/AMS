---
namespace: "Ams.Cli.Services"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Services/DspConfigService.cs"
access_modifier: "internal"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
---
# DspConfigService::GetConfigFilePath
**Path**: `Projects/AMS/host/Ams.Cli/Services/DspConfigService.cs`

## Summary
**Returns the resolved filesystem path to the DSP config file used by the service’s load and save flows.**

GetConfigFilePath is an internal static helper on DspConfigService that delegates path computation to Resolve and returns the resolved string. With complexity 2, the implementation is minimal and likely includes only a simple conditional path-selection/fallback branch. It is invoked by both LoadAsync and SaveAsync, centralizing config-path resolution for read/write consistency.


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

