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
# IBookParser::ParseAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`


#### [[IBookParser.ParseAsync]]
##### What it does:
<member name="M:Ams.Core.Runtime.Book.IBookParser.ParseAsync(System.String,System.Threading.CancellationToken)">
    <summary>
    Extracts raw text content from the specified file.
    This method performs format-specific parsing but does not
    perform any text processing or indexing.
    </summary>
    <param name="filePath">Path to the file to parse</param>
    <param name="cancellationToken">Cancellation token</param>
    <returns>Extracted text content and optional metadata</returns>
    <exception cref="T:System.IO.FileNotFoundException">File does not exist</exception>
    <exception cref="T:System.InvalidOperationException">File format not supported</exception>
    <exception cref="T:System.IO.IOException">File could not be read</exception>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<BookParseResult> ParseAsync(string filePath, CancellationToken cancellationToken = default(CancellationToken))
```

