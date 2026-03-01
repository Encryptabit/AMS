---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PronunciationLexiconCache::SanitizeFileName
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`

## Summary
**It converts a model name into a filesystem-safe filename token with stable defaulting.**

`SanitizeFileName` normalizes arbitrary input into a safe filename stem for cache files. It returns `"default"` when input is null/whitespace; otherwise it scans characters and keeps only letters, digits, `-`, `_`, and `.`, replacing every other character with `_` via a `StringBuilder`. It trims leading/trailing underscores from the built value and again falls back to `"default"` if the result is empty.


#### [[PronunciationLexiconCache.SanitizeFileName]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string SanitizeFileName(string value)
```

**Called-by <-**
- [[PronunciationLexiconCache.ResolveCacheFilePath]]

