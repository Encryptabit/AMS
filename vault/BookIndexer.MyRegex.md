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
# BookIndexer::MyRegex
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`


#### [[BookIndexer.MyRegex]]
##### What it does:
<member name="M:Ams.Core.Runtime.Documents.BookIndexer.MyRegex">
    <remarks>
    Pattern:<br/>
    <code>^\\s*(chapter\\b|prologue\\b|epilogue\\b|prelude\\b|foreword\\b|introduction\\b|afterword\\b|appendix\\b)</code><br/>
    Options:<br/>
    <code>RegexOptions.IgnoreCase | RegexOptions.Compiled</code><br/>
    Explanation:<br/>
    <code>
    ○ Match if at the beginning of the string.<br/>
    ○ Match a whitespace character atomically any number of times.<br/>
    ○ 1st capture group.<br/>
        ○ Match with 7 alternative expressions, atomically.<br/>
            ○ Match a sequence of expressions.<br/>
                ○ Match a character in the set [Cc].<br/>
                ○ Match a character in the set [Hh].<br/>
                ○ Match a character in the set [Aa].<br/>
                ○ Match a character in the set [Pp].<br/>
                ○ Match a character in the set [Tt].<br/>
                ○ Match a character in the set [Ee].<br/>
                ○ Match a character in the set [Rr].<br/>
                ○ Match if at a word boundary.<br/>
            ○ Match a sequence of expressions.<br/>
                ○ Match a character in the set [Pp].<br/>
                ○ Match a character in the set [Rr].<br/>
                ○ Match a character in the set [Oo].<br/>
                ○ Match a character in the set [Ll].<br/>
                ○ Match a character in the set [Oo].<br/>
                ○ Match a character in the set [Gg].<br/>
                ○ Match a character in the set [Uu].<br/>
                ○ Match a character in the set [Ee].<br/>
                ○ Match if at a word boundary.<br/>
            ○ Match a sequence of expressions.<br/>
                ○ Match a character in the set [Ee].<br/>
                ○ Match a character in the set [Pp].<br/>
                ○ Match a character in the set [Ii\u0130].<br/>
                ○ Match a character in the set [Ll].<br/>
                ○ Match a character in the set [Oo].<br/>
                ○ Match a character in the set [Gg].<br/>
                ○ Match a character in the set [Uu].<br/>
                ○ Match a character in the set [Ee].<br/>
                ○ Match if at a word boundary.<br/>
            ○ Match a sequence of expressions.<br/>
                ○ Match a character in the set [Pp].<br/>
                ○ Match a character in the set [Rr].<br/>
                ○ Match a character in the set [Ee].<br/>
                ○ Match a character in the set [Ll].<br/>
                ○ Match a character in the set [Uu].<br/>
                ○ Match a character in the set [Dd].<br/>
                ○ Match a character in the set [Ee].<br/>
                ○ Match if at a word boundary.<br/>
            ○ Match a sequence of expressions.<br/>
                ○ Match a character in the set [Ff].<br/>
                ○ Match a character in the set [Oo].<br/>
                ○ Match a character in the set [Rr].<br/>
                ○ Match a character in the set [Ee].<br/>
                ○ Match a character in the set [Ww].<br/>
                ○ Match a character in the set [Oo].<br/>
                ○ Match a character in the set [Rr].<br/>
                ○ Match a character in the set [Dd].<br/>
                ○ Match if at a word boundary.<br/>
            ○ Match a sequence of expressions.<br/>
                ○ Match a character in the set [Ii\u0130].<br/>
                ○ Match a character in the set [Nn].<br/>
                ○ Match a character in the set [Tt].<br/>
                ○ Match a character in the set [Rr].<br/>
                ○ Match a character in the set [Oo].<br/>
                ○ Match a character in the set [Dd].<br/>
                ○ Match a character in the set [Uu].<br/>
                ○ Match a character in the set [Cc].<br/>
                ○ Match a character in the set [Tt].<br/>
                ○ Match a character in the set [Ii\u0130].<br/>
                ○ Match a character in the set [Oo].<br/>
                ○ Match a character in the set [Nn].<br/>
                ○ Match if at a word boundary.<br/>
            ○ Match a sequence of expressions.<br/>
                ○ Match a character in the set [Aa].<br/>
                ○ Match with 2 alternative expressions, atomically.<br/>
                    ○ Match a sequence of expressions.<br/>
                        ○ Match an empty string.<br/>
                        ○ Match a character in the set [Tt].<br/>
                        ○ Match a character in the set [Ee].<br/>
                        ○ Match a character in the set [Rr].<br/>
                        ○ Match a character in the set [Ww].<br/>
                        ○ Match a character in the set [Oo].<br/>
                        ○ Match a character in the set [Rr].<br/>
                        ○ Match a character in the set [Dd].<br/>
                        ○ Match if at a word boundary.<br/>
                    ○ Match a sequence of expressions.<br/>
                        ○ Match a character in the set [Pp] exactly 2 times.<br/>
                        ○ Match a character in the set [Ee].<br/>
                        ○ Match a character in the set [Nn].<br/>
                        ○ Match a character in the set [Dd].<br/>
                        ○ Match a character in the set [Ii\u0130].<br/>
                        ○ Match a character in the set [Xx].<br/>
                        ○ Match if at a word boundary.<br/>
    </code>
    </remarks>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Regex MyRegex()
```

