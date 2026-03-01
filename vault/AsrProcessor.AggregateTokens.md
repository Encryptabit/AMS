---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 4
tags:
  - method
---
# AsrProcessor::AggregateTokens
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`


#### [[AsrProcessor.AggregateTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<AsrToken> AggregateTokens<TToken>(IEnumerable<TToken> rawTokens, Func<TToken, double> startSelector, Func<TToken, double> endSelector, Func<TToken, string> textSelector)
```

**Calls ->**
- [[Flush]]
- [[AsrProcessor.HasExplicitWordBoundary]]
- [[AsrProcessor.IsSpecialToken]]
- [[AsrProcessor.NormalizeTokenText]]

**Called-by <-**
- [[AsrProcessor.AggregateTokens_2]]

