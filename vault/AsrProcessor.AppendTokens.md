---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# AsrProcessor::AppendTokens
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`


#### [[AsrProcessor.AppendTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void AppendTokens(List<AsrToken> tokens, SegmentData segment)
```

**Calls ->**
- [[AsrProcessor.AggregateTokens_2]]

**Called-by <-**
- [[AsrProcessor.RunWhisperPassAsync]]

