---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
---
# SectionLocator::DetectSectionWindow
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Returns the detected section’s inclusive word-index bounds from ASR prefix tokens, or null if detection fails.**

DetectSectionWindow is a thin wrapper over `DetectSection(book, asrTokens, prefixTokenCount)`. It forwards inputs unchanged, then projects a non-null `SectionRange` to its word bounds tuple `(sec.StartWord, sec.EndWord)` and returns `null` when no section is detected. The method contains no additional scoring or validation logic beyond delegation.


#### [[SectionLocator.DetectSectionWindow]]
##### What it does:
<member name="M:Ams.Core.Processors.Alignment.Anchors.SectionLocator.DetectSectionWindow(Ams.Core.Runtime.Book.BookIndex,System.Collections.Generic.IReadOnlyList{System.String},System.Int32)">
    <summary>
    Returns the word-index window [start,end] of the detected section, or null.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static (int startWord, int endWord)? DetectSectionWindow(BookIndex book, IReadOnlyList<string> asrTokens, int prefixTokenCount = 8)
```

**Calls ->**
- [[SectionLocator.DetectSection]]

