---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/AudioBuffer.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# AudioBuffer::UpdateMetadata
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/AudioBuffer.cs`

## Summary
**Replace the buffer’s current metadata object with a new one.**

`UpdateMetadata` performs a direct metadata replacement on the `AudioBuffer` by assigning the passed `AudioBufferMetadata` to the mutable `Metadata` property. It is a simple mutator with no merge behavior, validation, or defensive copy. The method’s effect is immediate and scoped to the current buffer instance.


#### [[AudioBuffer.UpdateMetadata]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void UpdateMetadata(AudioBufferMetadata metadata)
```

**Called-by <-**
- [[FfDecoder.Decode]]

