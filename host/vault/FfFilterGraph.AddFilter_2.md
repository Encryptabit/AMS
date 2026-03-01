---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfFilterGraph::AddFilter
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Adds a validated FFmpeg filter node (with optional key/value arguments) into the graph’s fluent clause chain.**

`AddFilter` is the core private fluent-builder helper that appends a filter clause to `FfFilterGraph._clauses` after guarding against `_customGraphOverride`, throwing `InvalidOperationException` when manual graph mode is already enabled. When `markFormatPinned` is true, it sets `_formatPinned` so later default-format insertion logic is suppressed. It delegates argument formatting/escaping to `SerializeArguments(kvPairs)` and emits either `name` or `name=args` depending on whether serialized arguments are empty, then returns `this` for chaining.


#### [[FfFilterGraph.AddFilter_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private FfFilterGraph AddFilter(string name, IEnumerable<(string Key, string Value)> kvPairs, bool markFormatPinned = false)
```

**Calls ->**
- [[FfFilterGraph.SerializeArguments]]

**Called-by <-**
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.AFormat]]
- [[FfFilterGraph.DynaudNorm]]

