using System.Text.RegularExpressions;

namespace Ams.Tests.Documentation;

public sealed class BlockerMapDocsTests
{
    private const string BlockerMapRelativePath = ".gsd/milestones/M001/slices/S02/S02-BLOCKER-MAP.md";
    private const string RequiredHeader = "| ID | Surface | Finding | Severity | M002 Impact | Blocker? (yes/no) | Recommended Action | Evidence | retire_in |";

    private static readonly (string Id, string Finding)[] RequiredHighBlockers =
    {
        ("B01", "Workstation Prep execution gap"),
        ("B02", "CLI-owned run/config/progress model"),
        ("B03", "Build-index seam duplication"),
        ("B04", "Missing typed module/run contracts")
    };

    private static readonly HashSet<string> AllowedRetireTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "S03",
        "S04",
        "defer"
    };

    [Fact]
    public void BlockerMap_Contains_RequiredSchemaHeader()
    {
        var lines = ReadRepoFile(BlockerMapRelativePath)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n');

        Assert.Contains(lines, line => string.Equals(line.Trim(), RequiredHeader, StringComparison.Ordinal));
    }

    [Fact]
    public void BlockerMap_Rows_Are_SchemaComplete_With_ExplicitHandoffFields()
    {
        var rows = ParseBlockerRows();

        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hasBlockerYes = false;
        var hasBlockerNo = false;

        foreach (var row in rows)
        {
            Assert.Matches("^B\\d{2}$", row.Id);
            Assert.True(seenIds.Add(row.Id),
                $"Duplicate blocker ID '{row.Id}' found at line {row.LineNumber}.");

            var blockerFlag = Normalize(row.Blocker);
            Assert.True(blockerFlag is "yes" or "no",
                $"Row {row.Id} has invalid blocker flag '{row.Blocker}'. Expected yes/no.");

            hasBlockerYes |= blockerFlag == "yes";
            hasBlockerNo |= blockerFlag == "no";

            Assert.False(string.IsNullOrWhiteSpace(row.Evidence),
                $"Row {row.Id} is missing Evidence column value.");
            Assert.False(string.IsNullOrWhiteSpace(row.RetireIn),
                $"Row {row.Id} is missing retire_in column value.");

            var retireTag = row.RetireIn.Trim();
            Assert.True(AllowedRetireTags.Contains(retireTag),
                $"Row {row.Id} has unsupported retire_in tag '{row.RetireIn}'. Expected one of: S03, S04, defer.");
        }

        Assert.True(hasBlockerYes, "Expected at least one blocker row with Blocker? = yes.");
        Assert.True(hasBlockerNo, "Expected at least one deferrable row with Blocker? = no.");
    }

    [Fact]
    public void BlockerMap_Contains_RequiredHighBlockers_With_StableIds()
    {
        var rows = ParseBlockerRows();

        foreach (var required in RequiredHighBlockers)
        {
            var row = rows.SingleOrDefault(r => string.Equals(r.Id, required.Id, StringComparison.OrdinalIgnoreCase));
            Assert.True(row is not null, $"Missing required high blocker row '{required.Id}'.");

            Assert.Contains(required.Finding, row!.Finding, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("high", Normalize(row.Severity));
            Assert.Equal("yes", Normalize(row.Blocker));
            Assert.NotEqual("defer", Normalize(row.RetireIn));
        }
    }

    [Fact]
    public void BlockerMap_Covers_Core_Cli_And_Workstation_Surfaces()
    {
        var rows = ParseBlockerRows();
        var coverage = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var surface = Normalize(row.Surface);
            if (surface.Contains("core", StringComparison.Ordinal))
            {
                coverage.Add("core");
            }

            if (surface.Contains("cli", StringComparison.Ordinal))
            {
                coverage.Add("cli");
            }

            if (surface.Contains("workstation", StringComparison.Ordinal))
            {
                coverage.Add("workstation");
            }
        }

        Assert.Contains("core", coverage);
        Assert.Contains("cli", coverage);
        Assert.Contains("workstation", coverage);
    }

    [Fact]
    public void BlockerMap_Deferrables_Include_Medium_And_Low_Severities()
    {
        var deferrables = ParseBlockerRows()
            .Where(row => string.Equals(Normalize(row.Blocker), "no", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(deferrables);
        Assert.Contains(deferrables, row => string.Equals(Normalize(row.Severity), "medium", StringComparison.Ordinal));
        Assert.Contains(deferrables, row => string.Equals(Normalize(row.Severity), "low", StringComparison.Ordinal));
    }

    [Fact]
    public void BlockerMap_HotspotAppendix_Contains_Auditable_LineCountEvidence()
    {
        var blockerMap = ReadRepoFile(BlockerMapRelativePath);

        Assert.Contains("## Appendix B — Hotspot line-count evidence", blockerMap);
        Assert.Contains("wc -l", blockerMap);
        Assert.Contains("host/Ams.Cli/Commands/PipelineCommand.cs", blockerMap);
        Assert.Contains("host/Ams.Workstation.Server/Services/BlazorWorkspace.cs", blockerMap);
    }

    [Fact]
    public void BlockerMap_References_S01_EngineeringContract_As_AuditInput()
    {
        var blockerMap = ReadRepoFile(BlockerMapRelativePath);

        Assert.Contains("Audit input baseline", blockerMap);
        Assert.Contains("`CODE-STYLE.md`", blockerMap);
    }

    private static IReadOnlyList<BlockerRow> ParseBlockerRows()
    {
        var markdown = ReadRepoFile(BlockerMapRelativePath);
        var lines = markdown
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n');

        var headerIndex = Array.FindIndex(lines,
            line => string.Equals(line.Trim(), RequiredHeader, StringComparison.Ordinal));

        Assert.True(headerIndex >= 0,
            $"Could not find blocker-map header '{RequiredHeader}'.");
        Assert.True(headerIndex + 1 < lines.Length,
            "Blocker-map table is missing the schema separator row.");
        Assert.True(lines[headerIndex + 1].Trim().StartsWith("| ---", StringComparison.Ordinal),
            "Blocker-map schema separator row must appear directly below the header.");

        var rows = new List<BlockerRow>();

        for (var i = headerIndex + 2; i < lines.Length; i++)
        {
            var trimmed = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                break;
            }

            if (!trimmed.StartsWith("|", StringComparison.Ordinal))
            {
                break;
            }

            if (trimmed.StartsWith("| ---", StringComparison.Ordinal))
            {
                continue;
            }

            var cells = ParseCells(trimmed, i + 1);
            rows.Add(new BlockerRow(
                Id: cells[0],
                Surface: cells[1],
                Finding: cells[2],
                Severity: cells[3],
                Impact: cells[4],
                Blocker: cells[5],
                RecommendedAction: cells[6],
                Evidence: cells[7],
                RetireIn: cells[8],
                LineNumber: i + 1));
        }

        Assert.NotEmpty(rows);
        return rows;
    }

    private static string[] ParseCells(string row, int lineNumber)
    {
        Assert.True(row.EndsWith("|", StringComparison.Ordinal),
            $"Malformed blocker-map row at line {lineNumber}: row must end with '|'.");

        var segments = row.Split('|');
        var parsedColumnCount = segments.Length - 2; // drop leading/trailing empty columns

        Assert.True(parsedColumnCount == 9,
            $"Malformed blocker-map row at line {lineNumber}: expected 9 columns but found {parsedColumnCount}. Row text: {row}");

        return segments
            .Skip(1)
            .Take(9)
            .Select(static cell => cell.Trim())
            .ToArray();
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        return File.ReadAllText(Path.Combine(repoRoot, relativePath));
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "CODE-STYLE.md")) &&
                Directory.Exists(Path.Combine(current.FullName, "host")) &&
                Directory.Exists(Path.Combine(current.FullName, ".gsd")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repo root containing CODE-STYLE.md and host/.");
    }

    private static string Normalize(string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim().ToLowerInvariant();
    }

    private sealed record BlockerRow(
        string Id,
        string Surface,
        string Finding,
        string Severity,
        string Impact,
        string Blocker,
        string RecommendedAction,
        string Evidence,
        string RetireIn,
        int LineNumber);
}
