---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "public"
complexity: 26
fan_in: 0
fan_out: 8
tags:
  - method
  - danger/high-complexity
---
# MfaPronunciationProvider::GetPronunciationsAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`

> [!danger] High Complexity (26)
> Cyclomatic complexity: 26. Consider refactoring into smaller methods.


#### [[MfaPronunciationProvider.GetPronunciationsAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<IReadOnlyDictionary<string, string[]>> GetPronunciationsAsync(IEnumerable<string> words, CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaPronunciationProvider.ComposeLexemePronunciations]]
- [[MfaPronunciationProvider.MergePronunciationMaps]]
- [[MfaPronunciationProvider.NormalizeVariantKey]]
- [[MfaPronunciationProvider.RunG2pWithProgressAsync]]
- [[PronunciationLexiconCache.GetManyAsync]]
- [[PronunciationLexiconCache.MergeAsync]]
- [[Log.Debug]]
- [[PronunciationHelper.SplitLexemeIntoWords]]

