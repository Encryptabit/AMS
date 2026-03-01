---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
---
# WhisperFactoryPool::Acquire
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Acquires a reference-counted shared `WhisperFactory` instance keyed by model/options and returns a disposable release handle.**

`Acquire` builds a `FactoryKey` from normalized `modelPath` (`Path.GetFullPath`) plus GPU/DTW-related options, then lock-protects access to the static `Entries` dictionary. If no cached entry exists for that key, it creates one via `new FactoryEntry(WhisperFactory.FromPath(...))`; in all cases it increments `RefCount`, returns the shared `WhisperFactory` through the `out` parameter, and hands back a `FactoryHandle` bound to that key/entry. The returned handle’s `Dispose` performs the matching reference decrement and conditional factory disposal.


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

