---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FilterGraphExecutor::CreateInputs
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Builds and validates per-input source state objects for all provided graph inputs.**

`CreateInputs` materializes executor-ready source state by transforming each `GraphInput` into a `GraphInputState` via `SetupSource`. It allocates a fixed result array sized to `inputs.Count`, validates each `input.Buffer` is non-null (throwing `ArgumentNullException` otherwise), and normalizes blank labels to deterministic defaults (`"in{i}"`). The method returns the populated array for subsequent graph wiring.


#### [[FilterGraphExecutor.CreateInputs]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private FfFilterGraphRunner.GraphInputState[] CreateInputs(IReadOnlyList<FfFilterGraphRunner.GraphInput> inputs)
```

**Calls ->**
- [[FilterGraphExecutor.SetupSource]]

**Called-by <-**
- [[FilterGraphExecutor..ctor]]

