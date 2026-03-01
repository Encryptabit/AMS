---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IPronunciationProvider.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
---
# NullPronunciationProvider::GetPronunciationsAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IPronunciationProvider.cs`


#### [[NullPronunciationProvider.GetPronunciationsAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<IReadOnlyDictionary<string, string[]>> GetPronunciationsAsync(IEnumerable<string> words, CancellationToken cancellationToken)
```

