---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# PronunciationLexiconCache::NormalizeEntries
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`

## Summary
**It cleans and canonicalizes cached pronunciation entries by dropping invalid keys and empty variant sets.**

`NormalizeEntries` rebuilds the input map into a new case-insensitive dictionary while filtering invalid data. It skips blank/whitespace lexeme keys, normalizes each variant list via `NormalizeVariants`, and only retains entries whose normalized variant array is non-empty. The result is a sanitized dictionary suitable for cache use.


#### [[PronunciationLexiconCache.NormalizeEntries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<string, string[]> NormalizeEntries(IReadOnlyDictionary<string, string[]> entries)
```

**Calls ->**
- [[PronunciationLexiconCache.NormalizeVariants]]

**Called-by <-**
- [[PronunciationLexiconCache.ReadCoreAsync]]

