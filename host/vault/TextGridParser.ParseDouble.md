---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/TextGridParser.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# TextGridParser::ParseDouble
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/TextGridParser.cs`

## Summary
**Safely extracts an invariant-culture floating-point value from a `key = value` text line.**

ParseDouble parses a numeric assignment line (e.g., `xmin = ...`) by splitting once on `'='` with trimmed entries. If the split does not produce exactly two parts, it returns `0`. Otherwise it attempts `double.TryParse` on the right-hand side using `NumberStyles.Float` and `CultureInfo.InvariantCulture`, returning the parsed value or `0` on failure.


#### [[TextGridParser.ParseDouble]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ParseDouble(string line)
```

**Called-by <-**
- [[TextGridParser.ParseIntervals]]

