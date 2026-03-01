---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/BookMetadataResetService.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 1
tags:
  - method
---
# BookMetadataResetService::RemoveBookScopedEntries
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/BookMetadataResetService.cs`


#### [[BookMetadataResetService.RemoveBookScopedEntries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int RemoveBookScopedEntries(string bookId)
```

**Calls ->**
- [[AmsAppDataPaths.Resolve]]

**Called-by <-**
- [[BookMetadataResetService.ResetCurrentBook]]

