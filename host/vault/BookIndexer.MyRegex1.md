---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
---
# BookIndexer::MyRegex1
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Provides the source-generated regex used to parse chapter titles into components for duplicate disambiguation.**

`MyRegex1()` is a `[GeneratedRegex]` partial factory method that returns the compiled, case-insensitive pattern used for chapter-title duplicate handling. Its regex `^(?<prefix>\s*chapter)(?<ws>\s+)(?<number>\d+)(?<suffix>\s*[A-Za-z]*)?$` captures normalized chapter components (`prefix`, spacing, numeric index, optional alphabetic suffix) and anchors the full string. The method supplies the static `ChapterDuplicateRegex` used by duplicate-title suffix logic.


#### [[BookIndexer.MyRegex1]]
##### What it does:
<member name="M:Ams.Core.Runtime.Documents.BookIndexer.MyRegex1">
    <remarks>
    Pattern:<br/>
    <code>^(?&lt;prefix&gt;\\s*chapter)(?&lt;ws&gt;\\s+)(?&lt;number&gt;\\d+)(?&lt;suffix&gt;\\s*[A-Za-z]*)?$</code><br/>
    Options:<br/>
    <code>RegexOptions.IgnoreCase | RegexOptions.Compiled</code><br/>
    Explanation:<br/>
    <code>
    ○ Match if at the beginning of the string.<br/>
    ○ "prefix" capture group.<br/>
        ○ Match a whitespace character atomically any number of times.<br/>
        ○ Match a character in the set [Cc].<br/>
        ○ Match a character in the set [Hh].<br/>
        ○ Match a character in the set [Aa].<br/>
        ○ Match a character in the set [Pp].<br/>
        ○ Match a character in the set [Tt].<br/>
        ○ Match a character in the set [Ee].<br/>
        ○ Match a character in the set [Rr].<br/>
    ○ "ws" capture group.<br/>
        ○ Match a whitespace character atomically at least once.<br/>
    ○ "number" capture group.<br/>
        ○ Match a Unicode digit greedily at least once.<br/>
    ○ Optional (greedy).<br/>
        ○ "suffix" capture group.<br/>
            ○ Match a whitespace character greedily any number of times.<br/>
            ○ Match a character in the set [A-Za-z\u0130\u212A] greedily any number of times.<br/>
    ○ Match if at the end of the string or if before an ending newline.<br/>
    </code>
    </remarks>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Regex MyRegex1()
```

