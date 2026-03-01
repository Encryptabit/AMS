---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
---
# IBookParser::CanParse
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`


#### [[IBookParser.CanParse]]
##### What it does:
<member name="M:Ams.Core.Runtime.Book.IBookParser.CanParse(System.String)">
    <summary>
    Determines if this parser can handle the specified file.
    </summary>
    <param name="filePath">Path to the file to check</param>
    <returns>True if this parser supports the file format</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
bool CanParse(string filePath)
```

