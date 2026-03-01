---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/TextGridParser.cs"
access_modifier: "private"
complexity: 12
fan_in: 2
fan_out: 2
tags:
  - method
---
# TextGridParser::ParseIntervals
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/TextGridParser.cs`


#### [[TextGridParser.ParseIntervals]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<TextGridInterval> ParseIntervals(string textGridPath, Func<string, bool> tierPredicate)
```

**Calls ->**
- [[TextGridParser.ExtractQuotedValue]]
- [[TextGridParser.ParseDouble]]

**Called-by <-**
- [[TextGridParser.ParsePhoneIntervals]]
- [[TextGridParser.ParseWordIntervals]]

