---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookAudio.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
---
# BookAudio::UnloadRoomtone
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookAudio.cs`

## Summary
**Clears cached roomtone audio state so memory is released and future access reloads it lazily.**

`UnloadRoomtone` resets the roomtone cache state by nulling `_roomtone` and clearing `_roomtoneLoaded`. This forces the next `Roomtone` access to perform a fresh lazy-load attempt instead of reusing prior data. It also emits a debug log entry with the current book ID for lifecycle traceability.


#### [[BookAudio.UnloadRoomtone]]
##### What it does:
<member name="M:Ams.Core.Runtime.Book.BookAudio.UnloadRoomtone">
    <summary>
    Unloads the roomtone buffer to free memory.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void UnloadRoomtone()
```

**Calls ->**
- [[Log.Debug]]

