---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
---
# FilterGraphExecutor::ConfigureChannelLayouts
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`


#### [[FilterGraphExecutor.ConfigureChannelLayouts]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void ConfigureChannelLayouts()
```

**Calls ->**
- [[AudioBufferMetadata.DescribeDefaultLayout]]
- [[FfUtils.FormatError]]

**Called-by <-**
- [[FilterGraphExecutor.ConfigureSinkFormat]]

