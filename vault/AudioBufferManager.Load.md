---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# AudioBufferManager::Load
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs`


#### [[AudioBufferManager.Load]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBufferContext Load(int index)
```

**Calls ->**
- [[AudioBufferManager.GetOrCreate]]

**Called-by <-**
- [[AudioBufferManager.TryMoveNext]]
- [[AudioBufferManager.TryMovePrevious]]

