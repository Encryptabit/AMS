---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
---
# WhisperFactoryPool::Acquire
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AsrProcessor.cs`


#### [[WhisperFactoryPool.Acquire]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IDisposable Acquire(string modelPath, WhisperFactoryOptions options, out WhisperFactory factory)
```

**Called-by <-**
- [[AsrProcessor.DetectLanguageInternalAsync]]
- [[AsrProcessor.RunWhisperPassAsync]]

