---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
---
# SectionLocator::DetectSectionWindow
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`


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

