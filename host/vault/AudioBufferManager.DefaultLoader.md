---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "private"
complexity: 3
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/error-handling
  - llm/validation
---
# AudioBufferManager::DefaultLoader
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs`

## Summary
**Attempts to decode an audio file into an `AudioBuffer` using descriptor settings, returning `null` on missing files or decode errors.**

`DefaultLoader` is the fallback `AudioBuffer` materializer used when no custom loader is supplied. It first checks `File.Exists(descriptor.Path)` and returns `null` if the source file is missing; otherwise it attempts `AudioProcessor.Decode` with `AudioDecodeOptions` populated from `descriptor.SampleRate` and `descriptor.Channels`. Decode failures are caught broadly, logged via `Log.Warn` with path/message, and converted to `null`, giving callers a non-throwing load contract.


#### [[AudioBufferManager.DefaultLoader]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private AudioBuffer DefaultLoader(AudioBufferDescriptor descriptor)
```

**Calls ->**
- [[Log.Warn]]
- [[AudioProcessor.Decode]]

