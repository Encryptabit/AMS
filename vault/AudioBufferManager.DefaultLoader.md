---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "private"
complexity: 3
fan_in: 0
fan_out: 2
tags:
  - method
---
# AudioBufferManager::DefaultLoader
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs`


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

