---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "private"
complexity: 4
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
---
# BookCacheStats::FormatBytes
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`

## Summary
**Formats a byte count into a readable size string with KB/MB/GB units.**

`FormatBytes` converts a raw byte count into a human-readable size string using binary thresholds (KB=1024, MB=1024², GB=1024³). It uses a switch expression to choose the largest matching unit (`GB`, `MB`, `KB`, or `bytes`) and formats scaled values to two decimals (`F2`) for non-byte units. Values below 1024 are emitted as integer byte strings without scaling.


#### [[BookCacheStats.FormatBytes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatBytes(long bytes)
```

