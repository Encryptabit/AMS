---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::SerializeArguments
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Builds a sanitized FFmpeg filter-argument string from key/value pairs.**

`SerializeArguments` converts filter option tuples into FFmpeg argument syntax by iterating `kvPairs`, discarding entries where `Key` or `Value` is null/whitespace, and emitting `key=escapedValue` segments. Each value is normalized through `Escape(...)` before concatenation to protect FFmpeg-reserved characters. The method returns the final colon-delimited string via `string.Join(":", parts)`.


#### [[FfFilterGraph.SerializeArguments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string SerializeArguments(IEnumerable<(string Key, string Value)> kvPairs)
```

**Calls ->**
- [[FfFilterGraph.Escape]]

**Called-by <-**
- [[FfFilterGraph.AddFilter_2]]

