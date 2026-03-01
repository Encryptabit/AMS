---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 20
fan_in: 2
fan_out: 0
tags:
  - method
  - danger/high-complexity
---
# AsrProcessor::ResolveDtwPreset
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

> [!danger] High Complexity (20)
> Cyclomatic complexity: 20. Consider refactoring into smaller methods.


#### [[AsrProcessor.ResolveDtwPreset]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static WhisperAlignmentHeadsPreset? ResolveDtwPreset(string modelPath)
```

**Called-by <-**
- [[AsrProcessor.CreateFactoryOptions]]
- [[AsrProcessor.IsDtwEffectivelyEnabled]]

