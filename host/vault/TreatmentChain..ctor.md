---
namespace: "Ams.Cli.Models"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Models/TreatmentModels.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
---
# TreatmentChain::.ctor
**Path**: `Projects/AMS/host/Ams.Cli/Models/TreatmentModels.cs`

## Summary
**Creates a default `TreatmentChain` instance with unset metadata and an empty `Nodes` collection.**

The parameterless `TreatmentChain` constructor is a convenience overload on the positional record that delegates directly to the primary constructor via `this(null, null, null, null, null, null, Array.Empty<TreatmentNode>())`. This initializes all optional scalar fields to `null` while ensuring `Nodes` is non-null and empty. It is O(1), with no control flow or validation logic.


#### [[TreatmentChain..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public TreatmentChain()
```

