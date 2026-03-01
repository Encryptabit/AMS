---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/validation
---
# StreamingEncoderSink::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Creates a streaming encoder sink bound to an output stream and audio encode options.**

This constructor sets up the managed dependencies used by the streaming FFmpeg sink. It validates both `output` and `options` (throwing `ArgumentNullException` when null) and stores them in `_output` and `_options` for deferred encoder initialization. It intentionally does not allocate native codec/muxer resources until `Initialize(...)` is called.


#### [[StreamingEncoderSink..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public StreamingEncoderSink(Stream output, AudioEncodeOptions options)
```

