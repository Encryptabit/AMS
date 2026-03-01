---
namespace: "Ams.Core.Runtime.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
---
# AudioBufferManager::Deallocate
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Audio/AudioBufferManager.cs`


#### [[AudioBufferManager.Deallocate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Deallocate(string bufferId)
```

**Calls ->**
- [[Log.Debug]]
- [[AudioBufferContext.Unload]]

**Called-by <-**
- [[PolishService.PersistCorrectedBuffer]]

