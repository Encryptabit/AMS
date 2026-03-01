---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::Escape
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Escapes FFmpeg-special characters in an argument value so it can be safely embedded in serialized filter options.**

`Escape` is a static string sanitizer used for FFmpeg filter argument encoding. It short-circuits on `string.IsNullOrEmpty(value)` and otherwise returns a transformed string via chained `Replace` calls that backslash-escape reserved characters (`\`, `:`, `,`, `;`, `[`, `]`, and `'`). The method is deterministic, allocation-only, and performs no exception handling or external I/O.


#### [[FfFilterGraph.Escape]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string Escape(string value)
```

**Called-by <-**
- [[FfFilterGraph.SerializeArguments]]

