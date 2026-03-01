---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 20
fan_in: 2
fan_out: 0
tags:
  - method
  - danger/high-complexity
  - llm/factory
  - llm/utility
  - llm/validation
---
# AsrProcessor::ResolveDtwPreset
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

> [!danger] High Complexity (20)
> Cyclomatic complexity: 20. Consider refactoring into smaller methods.

## Summary
**Maps a Whisper model path to the DTW alignment-head preset required for safe DTW timestamp configuration.**

`ResolveDtwPreset` infers a `WhisperAlignmentHeadsPreset` from the model filename, returning `null` immediately for null/whitespace input. It normalizes the name using `Path.GetFileName(modelPath).ToLowerInvariant().Replace('_', '-')`, then applies an ordered set of `Contains`/`Equals` checks from most specific variants (for example `large-v3-turbo`) to broader families (`large`, `medium`, `small`, `base`, `tiny`), including legacy large aliases like `ggml-large.bin` and `large.bin`. The ordered matching prevents generic patterns from shadowing more specific presets, and unknown names fall back to `null`.


#### [[AsrProcessor.ResolveDtwPreset]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static WhisperAlignmentHeadsPreset? ResolveDtwPreset(string modelPath)
```

**Called-by <-**
- [[AsrProcessor.CreateFactoryOptions]]
- [[AsrProcessor.IsDtwEffectivelyEnabled]]

