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
# BookIndexer::MyRegex2
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`


#### [[BookIndexer.MyRegex2]]
##### What it does:
<member name="M:Ams.Core.Runtime.Documents.BookIndexer.MyRegex2">
    <remarks>
    Pattern:<br/>
    <code>^\\s*((\\d+|[ivxlcdm]+)\\s*[-–.:]\\s*[a-zA-Z])</code><br/>
    Options:<br/>
    <code>RegexOptions.IgnoreCase | RegexOptions.Compiled</code><br/>
    Explanation:<br/>
    <code>
    ○ Match if at the beginning of the string.<br/>
    ○ Match a whitespace character atomically any number of times.<br/>
    ○ 1st capture group.<br/>
        ○ 2nd capture group.<br/>
            ○ Match with 2 alternative expressions.<br/>
                ○ Match a Unicode digit atomically at least once.<br/>
                ○ Match a character in the set [CDILMVXcdilmvx\u0130] atomically at least once.<br/>
        ○ Match a whitespace character atomically any number of times.<br/>
        ○ Match a character in the set [-.:\u2013].<br/>
        ○ Match a whitespace character atomically any number of times.<br/>
        ○ Match a character in the set [A-Za-z\u0130\u212A].<br/>
    </code>
    </remarks>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Regex MyRegex2()
```

