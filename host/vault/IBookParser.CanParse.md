---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/validation
---
# IBookParser::CanParse
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`

## Summary
**Declares the parser API for checking whether a file path is supported for parsing.**

`CanParse` is an interface contract member on `IBookParser` with no implementation body, defining capability probing by file path. It standardizes a boolean preflight check so callers can determine parser support before invoking parse operations. Concrete parsers decide their own extension/content heuristics and failure behavior.


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

