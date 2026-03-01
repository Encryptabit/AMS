---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookDocuments.cs"
access_modifier: "internal"
complexity: 1
fan_in: 0
fan_out: 3
tags:
  - method
---
# BookDocuments::.ctor
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookDocuments.cs`


#### [[BookDocuments..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal BookDocuments(BookContext context, IArtifactResolver resolver)
```

**Calls ->**
- [[IArtifactResolver.GetBookIndexFile]]
- [[IArtifactResolver.LoadBookIndex]]
- [[IArtifactResolver.SaveBookIndex]]

