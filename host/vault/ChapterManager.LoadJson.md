---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/error-handling
---
# ChapterManager::LoadJson
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Loads and deserializes a JSON file into a non-null typed object, failing fast when deserialization yields null.**

`LoadJson<T>` is a strict JSON loader that synchronously reads file contents and deserializes into `T` using `JsonOptions`. It calls `JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions)` and treats a `null` deserialization result as failure, throwing `InvalidOperationException` with type/path context. The method does not catch IO/parse exceptions, so they propagate naturally.


#### [[ChapterManager.LoadJson]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static T LoadJson<T>(string path)
```

**Called-by <-**
- [[ChapterManager.CreateContext]]

