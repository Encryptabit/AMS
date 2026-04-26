using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services;
using Xunit.Sdk;

namespace Ams.Tests.Workstation.Shell;

[CollectionDefinition(nameof(WorkstationMobileHardeningPersistenceCollection), DisableParallelization = true)]
public sealed class WorkstationMobileHardeningPersistenceCollection
{
}

[Collection(nameof(WorkstationMobileHardeningPersistenceCollection))]
public sealed class WorkstationMobileHardeningContractTests
{
    private static readonly object ReviewedStatusArtifactGate = new();

    [Fact]
    public void ReviewedStatusService_UsesCanonicalArtifactPathAndBookScopedSchema()
    {
        using var harness = PersistenceHarness.Create("mobile-hardening-reviewed");
        var service = new ReviewedStatusService(harness.Workspace, harness.ReviewedStatusBasePath);

        var filePath = InvokePrivate<string>(service, "GetFilePath");
        var expectedPath = Path.Combine(harness.ReviewedStatusBasePath, "reviewed-status.json");

        Assert.Equal(NormalizePath(expectedPath), NormalizePath(filePath));
        Assert.Equal("reviewed-status.json", Path.GetFileName(filePath));

        Assert.False(
            Regex.IsMatch(Path.GetFileName(filePath), @"reviewed-status-v[0-9]+\.json", RegexOptions.IgnoreCase),
            $"Reviewed status artifact path unexpectedly introduced version seam: '{filePath}'.");

        lock (ReviewedStatusArtifactGate)
        {
            var hadExistingFile = File.Exists(filePath);
            var existingContent = hadExistingFile ? File.ReadAllText(filePath) : null;
            var artifactDirectory = Path.GetDirectoryName(filePath)
                ?? throw new XunitException($"Could not determine directory for reviewed status artifact '{filePath}'.");

            Directory.CreateDirectory(artifactDirectory);
            var versionedBefore = Directory
                .EnumerateFiles(artifactDirectory, "reviewed-status-v*.json", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            try
            {
                service.ResetCurrentBook();
                service.SetReviewed("chapter-01", reviewed: true);

                Assert.True(
                    File.Exists(filePath),
                    $"Expected reviewed status artifact to be written at '{filePath}'.");

                using var document = JsonDocument.Parse(File.ReadAllText(filePath));
                Assert.True(
                    document.RootElement.TryGetProperty(harness.BookId, out var bookEntry),
                    $"Expected reviewed status artifact to contain book id '{harness.BookId}'.");

                Assert.True(
                    bookEntry.TryGetProperty("chapter-01", out var chapterEntry),
                    "Expected reviewed status artifact to contain chapter key 'chapter-01'.");

                Assert.True(
                    chapterEntry.TryGetProperty("Reviewed", out var reviewedFlag)
                    && reviewedFlag.ValueKind is JsonValueKind.True,
                    "Expected reviewed entry schema to preserve boolean 'Reviewed' field.");

                Assert.True(
                    chapterEntry.TryGetProperty("Timestamp", out var timestamp)
                    && timestamp.ValueKind is JsonValueKind.String
                    && DateTimeOffset.TryParse(timestamp.GetString(), out _),
                    "Expected reviewed entry schema to preserve parseable string 'Timestamp' field.");
            }
            finally
            {
                RestoreFile(filePath, hadExistingFile, existingContent);
            }

            var versionedAfter = Directory
                .EnumerateFiles(artifactDirectory, "reviewed-status-v*.json", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            Assert.Equal(versionedBefore, versionedAfter);
        }
    }

    [Fact]
    public void CrxService_PathResolvers_KeepCanonicalCrxArtifactNames()
    {
        using var harness = PersistenceHarness.Create("mobile-hardening-crx-paths");
        var service = harness.CreateCrxService();

        var jsonPath = InvokePrivate<string>(service, "GetCrxJsonPath", false);
        var excelPath = InvokePrivate<string>(service, "GetCrxExcelPath", false);

        var expectedCrxDirectory = Path.Combine(harness.RootPath, "CRX");
        var expectedBookName = Path.GetFileName(harness.RootPath.TrimEnd(Path.DirectorySeparatorChar));
        var expectedJsonPath = Path.Combine(expectedCrxDirectory, $"{expectedBookName}_CRX.json");
        var expectedExcelPath = Path.Combine(expectedCrxDirectory, $"{expectedBookName}_CRX.xlsx");

        Assert.Equal(NormalizePath(expectedJsonPath), NormalizePath(jsonPath));
        Assert.Equal(NormalizePath(expectedExcelPath), NormalizePath(excelPath));

        Assert.False(
            Regex.IsMatch(Path.GetFileName(jsonPath), @"_CRX[-_]?v[0-9]+\.json", RegexOptions.IgnoreCase),
            $"CRX JSON artifact name unexpectedly introduced a version seam: '{jsonPath}'.");

        Assert.False(
            Regex.IsMatch(Path.GetFileName(excelPath), @"_CRX[-_]?v[0-9]+\.xlsx", RegexOptions.IgnoreCase),
            $"CRX Excel artifact name unexpectedly introduced a version seam: '{excelPath}'.");

        Assert.False(
            Directory.Exists(expectedCrxDirectory),
            "Path resolver should remain side-effect free when createDir=false.");
    }

    [Fact]
    public void CrxService_JsonRoundTrip_PreservesShouldBeReadAsSchemaFields()
    {
        using var harness = PersistenceHarness.Create("mobile-hardening-crx-schema");
        var service = harness.CreateCrxService();

        var entry = new CrxEntry(
            ErrorNumber: 7,
            Chapter: "Chapter 01",
            Timecode: "00:00:11",
            ErrorType: "MR",
            Comments: "Schema seam contract check",
            SentenceId: 101,
            StartTime: 11.25,
            EndTime: 12.75,
            AudioFile: "007.wav",
            CreatedAt: DateTime.UtcNow,
            ShouldBe: "Corrected phrase",
            ReadAs: "Recorded phrase");

        InvokePrivate(service, "WriteJsonEntries", new List<CrxEntry> { entry });

        var persisted = service.GetEntries();
        var persistedEntry = Assert.Single(persisted);

        Assert.Equal(entry.ErrorNumber, persistedEntry.ErrorNumber);
        Assert.Equal(entry.ShouldBe, persistedEntry.ShouldBe);
        Assert.Equal(entry.ReadAs, persistedEntry.ReadAs);

        var jsonPath = InvokePrivate<string>(service, "GetCrxJsonPath", false);
        Assert.True(File.Exists(jsonPath), $"Expected CRX JSON artifact at '{jsonPath}'.");

        using var document = JsonDocument.Parse(File.ReadAllText(jsonPath));
        var payloadEntry = Assert.Single(document.RootElement.EnumerateArray());

        Assert.Equal(entry.ShouldBe, payloadEntry.GetProperty("ShouldBe").GetString());
        Assert.Equal(entry.ReadAs, payloadEntry.GetProperty("ReadAs").GetString());
    }

    private static void RestoreFile(string path, bool hadExistingFile, string? existingContent)
    {
        if (hadExistingFile)
        {
            File.WriteAllText(path, existingContent ?? string.Empty);
            return;
        }

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static string NormalizePath(string path)
        => path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

    private static void InvokePrivate(object instance, string methodName, params object?[] args)
    {
        var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new XunitException($"Missing private method '{methodName}' on '{instance.GetType().FullName}'.");

        method.Invoke(instance, args);
    }

    private static T InvokePrivate<T>(object instance, string methodName, params object?[] args)
    {
        var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new XunitException($"Missing private method '{methodName}' on '{instance.GetType().FullName}'.");

        var value = method.Invoke(instance, args);

        if (value is T typed)
        {
            return typed;
        }

        throw new XunitException(
            $"Private method '{methodName}' on '{instance.GetType().FullName}' returned unexpected type '{value?.GetType().FullName ?? "null"}'. Expected '{typeof(T).FullName}'.");
    }

    private sealed class PersistenceHarness : IDisposable
    {
        private PersistenceHarness(string rootPath, string reviewedStatusBasePath, BlazorWorkspace workspace)
        {
            RootPath = rootPath;
            ReviewedStatusBasePath = reviewedStatusBasePath;
            BookId = Path.GetFileName(rootPath.TrimEnd(Path.DirectorySeparatorChar));
            Workspace = workspace;
        }

        public string RootPath { get; }

        public string ReviewedStatusBasePath { get; }

        public string BookId { get; }

        public BlazorWorkspace Workspace { get; }

        public CrxService CreateCrxService()
            => new(Workspace, new AudioExportService(Workspace));

        public static PersistenceHarness Create(string prefix)
        {
            var rootPath = Path.Combine(Path.GetTempPath(), $"{prefix}-{Guid.NewGuid():N}");
            Directory.CreateDirectory(rootPath);
            File.WriteAllText(Path.Combine(rootPath, "book-index.json"), "{}");

            var workspaceStatePath = Path.Combine(rootPath, ".workstation-state.json");
            var workspace = new BlazorWorkspace(workspaceStatePath, loadPersistedState: false);
            var reviewedStatusBasePath = Path.Combine(rootPath, ".test-appdata", "workstation");

            Assert.True(
                workspace.SetWorkingDirectory(rootPath),
                $"Expected workspace to initialize for root '{rootPath}'.");

            workspace.SetPrecomputePeaksInBackground(false);

            return new PersistenceHarness(rootPath, reviewedStatusBasePath, workspace);
        }

        public void Dispose()
        {
            Workspace.Dispose();

            try
            {
                if (Directory.Exists(RootPath))
                {
                    Directory.Delete(RootPath, recursive: true);
                }
            }
            catch
            {
                // Best-effort temp workspace cleanup.
            }
        }
    }
}
