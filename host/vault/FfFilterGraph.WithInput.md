---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfFilterGraph::WithInput
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Adds a uniquely named secondary input buffer to the filter graph for later routing/mixing operations.**

This fluent method registers an additional labeled input `AudioBuffer` into the graph’s `_inputs` collection for multi-input filter scenarios. It enforces non-null buffer, non-blank label, and uniqueness of label (case-insensitive), throwing on invalid or duplicate registrations. After validation it delegates to `AddInput(buffer, label)` and returns `this` to continue chain composition.


#### [[FfFilterGraph.WithInput]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.WithInput(Ams.Core.Artifacts.AudioBuffer,System.String)">
    <summary>
    Register another labeled input buffer (useful for sidechains).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph WithInput(AudioBuffer buffer, string label)
```

**Calls ->**
- [[FfFilterGraph.AddInput]]

