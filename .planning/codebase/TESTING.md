# Testing Patterns

**Analysis Date:** 2026-02-06

## Test Framework

**Runner:**
- xUnit 2.9.3 (`host/Ams.Tests/Ams.Tests.csproj`)
- xunit.runner.visualstudio 3.1.5
- Microsoft.NET.Test.Sdk 18.0.1

**Assertion Library:**
- xUnit built-in assertions
- Matchers: `Assert.Equal`, `Assert.NotNull`, `Assert.True`, `Assert.Contains`, `Assert.Collection`, `Assert.Throws<T>`, `Assert.ThrowsAsync<T>`, `Assert.Single`, `Assert.InRange`

**Run Commands:**
```bash
dotnet test host/Ams.Tests/Ams.Tests.csproj                          # Run all tests
dotnet test host/Ams.Tests/Ams.Tests.csproj --verbosity normal        # Verbose output
dotnet test host/Ams.Tests/Ams.Tests.csproj --filter BookParsingTests # Single class
```

## Test File Organization

**Location:**
- Separate test project: `host/Ams.Tests/`
- Mirror structure to source with subdirectories: `Services/`, `Audio/`, `Common/`, `Prosody/`

**Naming:**
- ClassName + "Tests" suffix: `AnchorDiscoveryTests`, `BookParsingTests`, `TxAlignTests`
- Multiple test classes per file allowed: `BookParsingTests`, `BookIndexAcceptanceTests`, `BookModelsTests` in `BookParsingTests.cs`

**Structure:**
```
host/Ams.Tests/
├── AnchorDiscoveryTests.cs          # Anchor algorithm tests
├── BookParsingTests.cs              # Book parsing + acceptance tests
├── TokenizerTests.cs                # Tokenization tests
├── TxAlignTests.cs                  # Transcript alignment tests
├── WavIoTests.cs                    # WAV file I/O tests
├── AudioProcessorFilterTests.cs     # Audio filter tests
├── Services/
│   └── Alignment/
│       └── AnchorComputeServiceTests.cs
├── Audio/
│   └── AsrAudioPreparerTests.cs
├── Common/
│   └── ChapterLabelResolverTests.cs
└── Prosody/
    └── PauseDynamicsServiceTests.cs
```

## Test Structure

**Suite Organization:**
```csharp
public class AnchorDiscoveryTests
{
    [Fact]
    public void UniqueTrigrams_ProduceAnchors()
    {
        // arrange
        var book = new List<string> { "the", "black", "forest", "was", "dark" };
        var asr = new List<string> { "the", "black", "forest", "felt", "dark" };
        var policy = new AnchorPolicy(NGram: 3, TargetPerTokens: 50, ...);

        // act
        var anchors = AnchorDiscovery.SelectAnchors(book, sent, asr, policy);

        // assert
        Assert.Contains(anchors, a => a.Bp == 1 && a.Ap == 1);
    }
}
```

**Patterns:**
- Arrange-Act-Assert (AAA) structure
- `[Fact]` for single-scenario tests
- `[Theory]` with `[InlineData]` for parameterized tests
- No `beforeEach`/`afterEach` setup; stateless tests preferred
- Temporary files created and cleaned up per test with try/finally

## Mocking

**Framework:**
- No mocking library (Moq, NSubstitute) detected
- Manual stub implementations used

**Patterns:**
```csharp
private sealed class StubPronunciationProvider : IPronunciationProvider
{
    private readonly IReadOnlyDictionary<string, string[]> _map;

    public StubPronunciationProvider(IReadOnlyDictionary<string, string[]> map)
        => _map = map;

    public Task<IReadOnlyDictionary<string, string[]>> GetPronunciationsAsync(
        IEnumerable<string> words, CancellationToken cancellationToken)
        => Task.FromResult(_map);
}
```

**What to Mock:**
- External service interfaces (IPronunciationProvider)
- File system operations (via temp files, not mocked)

**What NOT to Mock:**
- Pure algorithm functions (test directly)
- Domain models and DTOs
- Static utility methods

## Fixtures and Factories

**Test Data:**
```csharp
// Static factory methods for common test data
private static BookIndex MakeBookIndex()
{
    var words = new List<BookWord>();
    for (int i = 0; i < 100; i++)
        words.Add(new BookWord($"w{i}", i, i / 5, i / 10, -1));

    var sections = new[]
    {
        new SectionRange(0, "Prologue", 1, "Heading", 0, 29, 0, 2),
        new SectionRange(1, "Chapter 14: Storm", 1, "Heading", 30, 99, 3, 9)
    };

    return new BookIndex(SourceFile: "fake.docx", ...);
}

// Helper for creating domain objects
private static PauseSpan CreateSpan(int leftSentenceId, int rightSentenceId,
    double startSec, double endSec, PauseClass pauseClass, bool hasGapHint = false)
{
    var duration = Math.Max(0d, endSec - startSec);
    return new PauseSpan(leftSentenceId, rightSentenceId, startSec, endSec, ...);
}
```

**Location:**
- Factory methods: defined inline in test files
- No shared fixtures directory (tests are self-contained)

## Coverage

**Requirements:**
- No enforced coverage threshold
- Coverage tracked for awareness via coverlet

**Configuration:**
- coverlet.collector 6.0.4 (`host/Ams.Tests/Ams.Tests.csproj`)
- No explicit exclusions configured

## Test Types

**Unit Tests:**
- Test single function/algorithm in isolation
- No DI container needed (direct instantiation)
- Examples: `AnchorDiscoveryTests`, `TokenizerTests`, `WindowBuilder` tests
- Fast: each test runs in milliseconds

**Integration Tests:**
- Test multiple components together (e.g., parsing + indexing + caching)
- Use real file system with temp files
- Examples: `BookIndexAcceptanceTests` (full round-trip), `TxAlignTests`

**E2E Tests:**
- Not currently implemented
- CLI and web UI tested manually

## Common Patterns

**Parameterized Testing:**
```csharp
[Theory]
[InlineData("Chapter 14 - Storm", 1)]
[InlineData("chapter fourteen storm", 1)]
[InlineData("Prologue", 0)]
public void ResolveSectionByTitle_NormalizesNumbers(string label, int expectedId)
{
    var book = MakeBookIndex();
    var resolved = SectionLocator.ResolveSectionByTitle(book, label);
    Assert.NotNull(resolved);
    Assert.Equal(expectedId, resolved!.Id);
}
```

**Async Testing:**
```csharp
[Fact]
public async Task Parser_Text_NoNormalization()
{
    var tmp = Path.GetTempFileName() + ".txt";
    try
    {
        await File.WriteAllTextAsync(tmp, "Title Line\r\n\r\nContent...");
        var result = await DocumentProcessor.ParseBookAsync(tmp);
        Assert.Equal("Title Line", result.Title);
    }
    finally
    {
        if (File.Exists(tmp)) File.Delete(tmp);
    }
}
```

**Exception Testing:**
```csharp
[Fact]
public async Task ComputeAnchorsAsync_NullContext_ThrowsArgumentNullException()
{
    var service = new AnchorComputeService();
    await Assert.ThrowsAsync<ArgumentNullException>(() =>
        service.ComputeAnchorsAsync(null!));
}
```

**Collection Assertions:**
```csharp
[Fact]
public void WindowsFromAnchors_AreClampedAndHalfOpen()
{
    var wins = WindowBuilder.Build(anchors, bookStart: 100, bookEnd: 119, asrStart: 0, asrEnd: 24);
    Assert.Collection(wins,
        w => Assert.Equal((100, 105, 0, 5), w),
        w => Assert.Equal((106, 112, 6, 14), w),
        w => Assert.Equal((113, 120, 15, 25), w));
}
```

**Resource Cleanup:**
```csharp
var cacheDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
var source = Path.GetTempFileName() + ".txt";
try { /* test */ }
finally
{
    if (File.Exists(source)) File.Delete(source);
    if (Directory.Exists(cacheDir)) Directory.Delete(cacheDir, true);
}
```

**Snapshot Testing:**
- Not used (prefer explicit assertions)

---

*Testing analysis: 2026-02-06*
*Update when test patterns change*
