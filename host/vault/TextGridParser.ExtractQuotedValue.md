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
---
# TextGridParser::ExtractQuotedValue
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/TextGridParser.cs`

## Summary
**Extracts a quoted field value from a TextGrid line when a complete quote pair is present.**

ExtractQuotedValue returns the substring between the first two double-quote characters in a line. It finds the first `"` and then searches for the next `"` after it; if either delimiter is missing, it returns `null`. On success, it extracts the quoted content via `Substring(firstQuote + 1, secondQuote - firstQuote - 1)`.


#### [[TextGridParser.ExtractQuotedValue]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ExtractQuotedValue(string line)
```

**Called-by <-**
- [[TextGridParser.ParseIntervals]]

