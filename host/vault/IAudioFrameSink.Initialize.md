---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/di
  - llm/utility
---
# IAudioFrameSink::Initialize
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Defines how an injected audio-frame sink is initialized with stream metadata and format context before processing begins.**

`Initialize` on `FfFilterGraphRunner.IAudioFrameSink` is an interface contract (no method body), so it represents a lifecycle boundary rather than local logic. It specifies the sink setup handshake with `templateMetadata`, `sampleRate`, and `channels` that must run before frame consumption. `FilterGraphExecutor` calls it once in its constructor when a sink is provided (`_frameSink.Initialize(_primaryMetadata, _sampleRate, _channels)`), prior to `Consume`/`Complete` flow.


#### [[IAudioFrameSink.Initialize]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void Initialize(AudioBufferMetadata templateMetadata, int sampleRate, int channels)
```

**Called-by <-**
- [[FilterGraphExecutor..ctor]]

