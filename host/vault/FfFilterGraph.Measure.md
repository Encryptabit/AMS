---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfFilterGraph::Measure
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Run the graph in log-capture mode and convert captured log lines into a caller-defined measurement result.**

This generic helper executes a measurement run and transforms the resulting FFmpeg logs into a typed result via a caller-supplied parser. The implementation captures logs through `CaptureLogs()` and returns `parser(logs)`, making it a central adapter between raw textual diagnostics and strongly-typed metrics. It also guards against a null parser (`ArgumentNullException.ThrowIfNull(parser)`) before execution.


#### [[FfFilterGraph.Measure]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.Measure``1(System.Func{System.Collections.Generic.IReadOnlyList{System.String},``0})">
    <summary>
    Execute the graph in measurement mode and parse the collected logs.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public T Measure<T>(Func<IReadOnlyList<string>, T> parser)
```

**Calls ->**
- [[FfFilterGraph.CaptureLogs]]

