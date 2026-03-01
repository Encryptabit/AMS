---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/IPronunciationProvider.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/async
  - llm/utility
  - llm/di
---
# NullPronunciationProvider::GetPronunciationsAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/IPronunciationProvider.cs`

## Summary
**Returns an empty pronunciation lookup as the default no-op implementation of `IPronunciationProvider`.**

`NullPronunciationProvider.GetPronunciationsAsync` is a non-throwing null-object implementation that ignores input words and cancellation and immediately returns an empty pronunciation map. It uses `Task.FromResult<IReadOnlyDictionary<string, string[]>>(new Dictionary<string, string[]>())` to provide a completed task without async work. This guarantees callers can rely on a valid dictionary result even when no real provider is configured.


#### [[NullPronunciationProvider.GetPronunciationsAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<IReadOnlyDictionary<string, string[]>> GetPronunciationsAsync(IEnumerable<string> words, CancellationToken cancellationToken)
```

