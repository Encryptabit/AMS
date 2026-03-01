---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
---
# BookIndexer::MyRegex1
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`


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

