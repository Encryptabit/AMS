---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookAudio.cs"
access_modifier: "private"
complexity: 3
fan_in: 0
fan_out: 3
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/error-handling
  - llm/validation
---
# BookAudio::LoadRoomtone
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookAudio.cs`

## Summary
**Attempts to decode the book’s roomtone file into an `AudioBuffer`, returning `null` when unavailable or decoding fails.**

`LoadRoomtone` resolves `RoomtonePath`, checks file existence, and returns `null` with a debug log when the asset is missing. If present, it attempts `AudioProcessor.Decode(path)` and logs successful load details (book ID, derived duration, sample rate). Decode failures are caught broadly, logged via `Log.Warn`, and converted to `null`, so callers get a non-throwing optional result.


#### [[BookAudio.LoadRoomtone]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private AudioBuffer LoadRoomtone()
```

**Calls ->**
- [[Log.Debug]]
- [[Log.Warn]]
- [[AudioProcessor.Decode]]

