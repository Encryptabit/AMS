---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrModels.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# AsrResponse::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrModels.cs`

## Summary
**Instantiate an ASR response object with required model version and null-safe token/segment collections.**

The `AsrResponse` constructor is the JSON-deserialization entry point (`[JsonConstructor]`) and enforces a non-null `modelVersion` by throwing `ArgumentNullException` when missing. It normalizes optional array inputs by replacing null `tokens`/`segments` with `Array.Empty<AsrToken>()` and `Array.Empty<AsrSegment>()`, guaranteeing non-null collection properties post-construction. This creates a defensive, allocation-light default state for downstream word/timing accessors.


#### [[AsrResponse..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AsrResponse(string modelVersion, AsrToken[] tokens = null, AsrSegment[] segments = null)
```

