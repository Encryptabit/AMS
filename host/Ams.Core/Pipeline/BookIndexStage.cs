using System.Text.Json;

namespace Ams.Core.Pipeline;

public sealed class BookIndexStage : StageRunner
{
    private readonly string _bookPath;
    private readonly BookIndexOptions _options;

    public BookIndexStage(string workDir, string bookPath, BookIndexOptions options)
        : base(workDir, "book-index")
    {
        _bookPath = bookPath ?? throw new ArgumentNullException(nameof(bookPath));
        _options = options ?? new BookIndexOptions();
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        if (!File.Exists(_bookPath))
            throw new FileNotFoundException($"Book file not found: {_bookPath}");

        var parser = new BookParser();
        if (!parser.CanParse(_bookPath))
        {
            var supportedExts = string.Join(", ", parser.SupportedExtensions);
            throw new InvalidOperationException($"Unsupported book format. Supported: {supportedExts}");
        }

        var indexer = new BookIndexer();
        var cache = new BookCache();

        // Try cache first
        BookIndex bookIndex = await cache.GetAsync(_bookPath, ct) ??
                              await BuildAsync(parser, indexer, cache, _bookPath, _options, ct);

        // Write canonical location expected by downstream stages
        var canonicalPath = Path.Combine(WorkDir, "book.index.json");
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await File.WriteAllTextAsync(canonicalPath, JsonSerializer.Serialize(bookIndex, jsonOptions), ct);

        // Persist params snapshot in stage dir for fingerprint parity
        await File.WriteAllTextAsync(Path.Combine(stageDir, "params.snapshot.json"), SerializeParams(_options), ct);

        return new Dictionary<string, string>
        {
            ["book_index"] = "../book.index.json",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        // Input hash = SHA256 of book file bytes (hex) + file length to resist rare collisions
        string inputHash;
        try
        {
            await using var fs = File.OpenRead(_bookPath);
            var sha = await System.Security.Cryptography.SHA256.HashDataAsync(fs, ct);
            inputHash = Convert.ToHexString(sha) + ":" + new FileInfo(_bookPath).Length.ToString();
        }
        catch
        {
            // Fall back to manifest input hash if book path inaccessible
            inputHash = manifest.Input.Sha256;
        }

        var paramsHash = ComputeHash(SerializeParams(_options));
        return new StageFingerprint(inputHash, paramsHash, new Dictionary<string, string>());
    }

    private static async Task<BookIndex> BuildAsync(
        IBookParser parser,
        IBookIndexer indexer,
        IBookCache cache,
        string path,
        BookIndexOptions options,
        CancellationToken ct)
    {
        var parsed = await parser.ParseAsync(path, ct);
        var idx = await indexer.CreateIndexAsync(parsed, path, options);
        await cache.SetAsync(idx, ct);
        return idx;
    }
}

