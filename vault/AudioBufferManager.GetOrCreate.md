---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# AudioBufferManager::GetOrCreate
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs`


#### [[AudioBufferManager.GetOrCreate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private AudioBufferContext GetOrCreate(AudioBufferDescriptor descriptor)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[AudioBufferManager.Load]]
- [[AudioBufferManager.Load_2]]

