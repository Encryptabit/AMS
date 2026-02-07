# Coding Conventions

**Analysis Date:** 2026-02-06

## Naming Patterns

**Files:**
- PascalCase.cs for all source files (`AsrCommand.cs`, `PipelineService.cs`)
- ClassName.Feature.cs for partial classes (`DocumentProcessor.Indexing.cs`, `AudioProcessor.Analysis.cs`)
- IInterfaceName.cs for interfaces (`IWorkspace.cs`, `IAsrService.cs`)
- ClassNameTests.cs for test files (`AnchorDiscoveryTests.cs`, `BookParsingTests.cs`)
- GlobalUsings.cs for project-wide using declarations

**Classes/Types:**
- PascalCase for classes, records, enums: `GenerateTranscriptCommand`, `PipelineStage`, `AsrEngine`
- I-prefix for interfaces (strict): `IWorkspace`, `IAsrService`, `IArtifactResolver`, `IAnchorComputeService`
- Sealed records for immutable data: `public sealed record AsrToken(...)`, `public sealed record ChapterOpenOptions { ... }`
- Suffix conventions: `*Command` (use-cases), `*Service` (stateless logic), `*Context` (resource holders), `*Manager` (lifecycle), `*Options` (config DTOs), `*Supervisor` (process management)

**Methods:**
- PascalCase for all public methods: `ExecuteAsync`, `TranscribeAsync`, `ComputeAnchorsAsync`
- Async suffix for async methods: `RunChapterAsync`, `ParseBookAsync`
- Factory prefixes: `CreateContext`, `CreateAsync`
- Resolve prefix for lookups: `ResolveWhisperModelAsync`, `ResolveSection`

**Variables:**
- camelCase for locals and parameters: `bookIndexFile`, `effectiveOptions`, `anchors`
- _camelCase with underscore prefix for private fields: `_asrService`, `_wordCache`, `_descriptors`
- UPPER_SNAKE_CASE not used (constants use PascalCase per C# convention)

## Code Style

**Formatting:**
- 4-space indentation (standard C#)
- Allman-style braces (opening brace on new line)
- Expression-bodied members for simple properties/methods:
  ```csharp
  public bool HasWordTimings => Tokens.Any(t => t.Duration > 0.0001);
  public MemoryStream ToWavStream(AudioEncodeOptions? options = null)
      => AudioProcessor.EncodeWavToStream(this, options);
  ```
- File-scoped namespaces: `namespace Ams.Cli.Commands;`
- Reasonable line lengths (~120 chars)

**Language Features:**
- Nullable reference types: enabled (`<Nullable>enable</Nullable>`)
- Implicit usings: enabled (`<ImplicitUsings>enable</ImplicitUsings>`)
- Unsafe blocks: allowed (`<AllowUnsafeBlocks>true</AllowUnsafeBlocks>`) for FFmpeg interop

**Linting:**
- No external linter (StyleCop, .editorconfig) detected
- Conventions enforced by team consistency

## Import Organization

**Order:**
1. System namespaces: `using System.CommandLine;`, `using System.Text;`
2. External libraries: `using Serilog;`, `using Microsoft.Extensions.Logging;`
3. Internal namespaces: `using Ams.Core.Application.Commands;`
4. Type aliases: `using AudioSentenceTiming = Ams.Core.Artifacts.SentenceTiming;`

**Global Usings:**
- Project-level `GlobalUsings.cs` files with `global using` statements
- `host/Ams.Core/GlobalUsings.cs`: Core domain namespaces
- `host/Ams.Cli/GlobalUsings.cs`: CLI + Core namespaces
- `host/Ams.Tests/GlobalUsings.cs`: Test + Core namespaces + `global using Xunit;`

**Path Aliases:**
- None (no path aliases configured)

## Error Handling

**Patterns:**
- Null validation via `ArgumentNullException.ThrowIfNull(chapter)` at method entry
- `InvalidOperationException` for missing artifacts/invalid state:
  ```csharp
  var book = context.Book.Documents.BookIndex
      ?? throw new InvalidOperationException("BookIndex is not loaded.");
  ```
- Try-catch at CLI command boundaries with logging and exit code:
  ```csharp
  catch (Exception ex) { Log.Error(ex, "asr run command failed"); context.ExitCode = 1; }
  ```
- Pipeline catches per-stage exceptions, returns result with error details
- `TryDelete()` pattern for safe file cleanup in finally blocks

**Null Safety:**
- Strict nullable reference types throughout
- Null-coalescing for defaults: `options ?? GenerateTranscriptOptions.Default`
- Nullable properties clearly marked: `string? Model { get; init; }`

## Logging

**Framework:**
- Serilog via static `Log` facade (`host/Ams.Core/Common/Log.cs`)
- Levels: Trace, Debug, Info, Warn, Error, Critical
- Configurable via `AMS_LOG_LEVEL` environment variable

**Patterns:**
- Per-class logger instances: `private static readonly ILogger Logger = Log.For<AnchorComputeService>();`
- Structured logging with named properties: `Log.Debug("ASR summary: ModelVersion={ModelVersion}, Tokens={TokenCount}", ...)`
- Log at service boundaries and state transitions
- Console sink (colored) + rolling file sink

## Comments

**When to Comment:**
- XML doc comments for public APIs (summaries, params, returns, exceptions)
- Minimal inline comments; code is self-documenting
- Comments explain "why" not "what"

**XML Documentation:**
```csharp
/// <summary>
/// Concatenates multiple AudioBuffer instances into a single new buffer.
/// All buffers must have matching SampleRate and Channels.
/// </summary>
/// <param name="buffers">The buffers to concatenate in order.</param>
/// <returns>A new AudioBuffer containing all samples sequentially.</returns>
/// <exception cref="ArgumentException">Thrown if buffers is empty.</exception>
```

**TODO Comments:**
- Format: `// TODO: description` (no username)
- Example: `// TODO: Review this process, there's a better way`

## Function Design

**Size:**
- Methods generally focused and under 50 lines
- Complex logic extracted into helper methods and processors

**Parameters:**
- CancellationToken on all async operations: `CancellationToken cancellationToken = default`
- Options objects for complex configuration: `GenerateTranscriptOptions`, `AnchorComputationOptions`
- Nullable options with defaults: `options ?? Default`

**Return Values:**
- Tuples for multiple returns: `private static (BookIndex Book, AsrResponse Asr) RequireBookAndAsr(...)`
- Nullable for optional returns: `public string? GetWord(int index)`
- Task<T> for async methods

## Module Design

**Namespaces:**
- Organized by functional layer:
  - `Ams.Core.Application.Commands` - Use-case commands
  - `Ams.Core.Runtime.{Book,Chapter,Workspace}` - Resource contexts
  - `Ams.Core.Services.Alignment` - Domain services
  - `Ams.Core.Processors.Alignment.{Anchors,Tx,Mfa}` - Algorithms
  - `Ams.Core.Artifacts` - DTOs

**DI Registration:**
- All services as singletons (stateless): `builder.Services.AddSingleton<IAsrService, AsrService>()`
- Commands as singletons: `builder.Services.AddSingleton<GenerateTranscriptCommand>()`
- Constructor injection with null guard:
  ```csharp
  public GenerateTranscriptCommand(IAsrService asrService)
  {
      _asrService = asrService ?? throw new ArgumentNullException(nameof(asrService));
  }
  ```

**Async Patterns:**
- All I/O operations are async with `ConfigureAwait(false)` in library code
- `SemaphoreSlim` for controlled parallelism in batch operations
- CancellationToken threading throughout

---

*Convention analysis: 2026-02-06*
*Update when patterns change*
