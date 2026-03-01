---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# AsrProcessor::AggregateTokens
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`


#### [[AsrProcessor.AggregateTokens_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<AsrToken> AggregateTokens(WhisperToken[] rawTokens)
```

**Calls ->**
- [[AsrProcessor.AggregateTokens]]

**Called-by <-**
- [[AsrProcessor.AppendTokens]]

