---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/BookCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# BookCommand::Sha256Hex
**Path**: `Projects/AMS/host/Ams.Cli/Commands/BookCommand.cs`

## Summary
**Computes the SHA-256 hash of a byte array and returns it as an uppercase hexadecimal string.**

`Sha256Hex` is a private static helper that instantiates `SHA256` via `SHA256.Create()`, computes the digest with `ComputeHash(bytes)`, and converts the result with `Convert.ToHexString(hash)`. It is synchronous and deterministic for the same input, returning uppercase hex output. In `RunVerifyAsync`, it hashes UTF-8 bytes of canonical JSON to produce the stable verification hash logged as the determinism fingerprint.


#### [[BookCommand.Sha256Hex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string Sha256Hex(byte[] bytes)
```

**Called-by <-**
- [[BookCommand.RunVerifyAsync]]

