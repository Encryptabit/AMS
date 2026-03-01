---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfFilterGraph::UseInput
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Selects which previously registered input buffer is used as the source for the next filter clauses.**

This method switches the active input label that subsequent fluent filter calls will target. It validates that the requested label has already been registered in `_inputs` (case-insensitive) and throws `InvalidOperationException` if not found. On success it updates `_inputLabel` and returns the same `FfFilterGraph` instance for continued chaining.


#### [[FfFilterGraph.UseInput]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.UseInput(System.String)">
    <summary>
    Selects which labeled input feeds the next chain (defaults to "main").
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph UseInput(string label)
```

