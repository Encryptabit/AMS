---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/TextGridParser.cs"
access_modifier: "private"
complexity: 12
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# TextGridParser::ParseIntervals
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/TextGridParser.cs`

## Summary
**Parses interval triples (xmin/xmax/text) from selected TextGrid tiers using a caller-supplied tier-name predicate.**

ParseIntervals streams a TextGrid file line-by-line, validating path existence first and throwing `FileNotFoundException` when missing. It tracks parser state (`capture`, `xmin`, `xmax`, `text`) and switches tier context on `item [`/`name =` lines, enabling capture only when `tierPredicate(ExtractQuotedValue(nameLine))` is true. While capturing, it parses `xmin`/`xmax` via `ParseDouble`, and on each `text =` line emits a `TextGridInterval(xmin, xmax, text)` when both bounds are present, then resets interval fields for the next entry. The result is an ordered list of intervals from matching tiers only.


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

