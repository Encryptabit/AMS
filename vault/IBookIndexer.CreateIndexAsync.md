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
# IBookIndexer::CreateIndexAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`


#### [[IBookIndexer.CreateIndexAsync]]
##### What it does:
<member name="M:Ams.Core.Runtime.Book.IBookIndexer.CreateIndexAsync(Ams.Core.Runtime.Book.BookParseResult,System.String,Ams.Core.Runtime.Book.BookIndexOptions,System.Threading.CancellationToken)">
    <summary>
    Creates a complete book index from parsed text content.
    This includes word tokenization, sentence/paragraph segmentation,
    and timing estimation for audio alignment.
    </summary>
    <param name="parseResult">Result from book parsing operation</param>
    <param name="sourceFile">Path to the original source file</param>
    <param name="options">Indexing configuration options</param>
    <param name="cancellationToken">Cancellation token</param>
    <returns>Complete book index with timing metadata</returns>
    <exception cref="T:System.ArgumentException">Invalid parse result or options</exception>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<BookIndex> CreateIndexAsync(BookParseResult parseResult, string sourceFile, BookIndexOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

