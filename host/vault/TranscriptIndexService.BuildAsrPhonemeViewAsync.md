---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/async
  - llm/utility
  - llm/validation
---
# TranscriptIndexService::BuildAsrPhonemeViewAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`

## Summary
**Resolves per-token ASR phoneme candidates in the filtered alignment token space using the configured pronunciation provider.**

This private async helper builds an ASR-token-indexed phoneme matrix by first fetching a pronunciation dictionary from `_pronunciationProvider.GetPronunciationsAsync(asr.Words, cancellationToken)`. It allocates `string[asrView.Tokens.Count][]`, maps each filtered ASR token back to its original word index (`FilteredToOriginalToken`), and enforces cancellation on each iteration via `ThrowIfCancellationRequested()`. For valid indices, it resolves the ASR word, normalizes it with `PronunciationHelper.NormalizeForLookup`, and stores pronunciation variants when present; otherwise it writes `Array.Empty<string>()` as a non-null fallback.


#### [[TranscriptIndexService.BuildAsrPhonemeViewAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<string[][]> BuildAsrPhonemeViewAsync(AsrResponse asr, AsrAnchorView asrView, CancellationToken cancellationToken)
```

**Calls ->**
- [[AsrResponse.GetWord]]
- [[IPronunciationProvider.GetPronunciationsAsync]]
- [[PronunciationHelper.NormalizeForLookup]]

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]

