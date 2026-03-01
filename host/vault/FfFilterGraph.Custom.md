---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 4
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfFilterGraph::Custom
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add a caller-supplied raw FFmpeg filter clause to the graph with basic non-empty validation.**

This method allows direct injection of a raw filtergraph clause into the internal `_clauses` list, bypassing typed helper argument construction. It validates input with `string.IsNullOrWhiteSpace` and throws `ArgumentException` when no clause is provided, then appends the clause and returns `this` for fluent chaining. It is used by higher-level helpers that need custom FFmpeg expressions not covered by dedicated wrappers.


#### [[FfFilterGraph.Custom]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.Custom(System.String)">
    <summary>
    Inject a raw filter clause when fluent helpers are insufficient.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph Custom(string rawClause)
```

**Called-by <-**
- [[AsrAudioPreparer.DownmixToMono]]
- [[AudioProcessor.DetectSilence]]
- [[AudioProcessor.FadeIn]]
- [[AudioProcessor.Trim]]

