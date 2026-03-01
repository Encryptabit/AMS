---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# AsrProcessor::IsDtwEffectivelyEnabled
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`


#### [[AsrProcessor.IsDtwEffectivelyEnabled]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsDtwEffectivelyEnabled(AsrOptions options)
```

**Calls ->**
- [[AsrProcessor.ResolveDtwPreset]]

**Called-by <-**
- [[AsrProcessor.ShouldRetryWithoutDtw]]

