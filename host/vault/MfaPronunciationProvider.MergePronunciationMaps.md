---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# MfaPronunciationProvider::MergePronunciationMaps
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`

## Summary
**It merges two pronunciation dictionaries into one, preferring non-empty entries from the second map on key collisions.**

`MergePronunciationMaps` creates a new case-insensitive dictionary and copies entries from `first` then `second`, but only when the variant array is non-null/non-empty (`Length > 0`). Because `second` is applied after `first`, overlapping lexemes are overwritten by `second`, making it the precedence source. The method filters out empty pronunciation payloads rather than preserving them.


#### [[MfaPronunciationProvider.MergePronunciationMaps]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyDictionary<string, string[]> MergePronunciationMaps(IReadOnlyDictionary<string, string[]> first, IReadOnlyDictionary<string, string[]> second)
```

**Called-by <-**
- [[MfaPronunciationProvider.GetPronunciationsAsync]]

