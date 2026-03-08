using System.IO;

namespace Ams.Core.Application.Mfa;

internal static class MfaWorkspaceResolver
{
    private const string MfaRootEnvVar = "MFA_ROOT_DIR";
    private const string WorkspaceEnvVar = "AMS_MFA_WORKSPACE";
    private const string WorkspacesEnvVar = "AMS_MFA_WORKSPACES";
    private const string LegacyWorkspaceName = "MFA";
    private const string PrimaryWorkspaceName = "MFA_1";

    public static string ResolvePreferredRoot(string? overrideRoot = null)
    {
        if (TryNormalizePath(overrideRoot, out var explicitRoot))
        {
            return EnsureWorkspace(explicitRoot);
        }

        if (TryNormalizePath(Environment.GetEnvironmentVariable(MfaRootEnvVar), out var fromMfaRoot))
        {
            return EnsureWorkspace(fromMfaRoot);
        }

        if (TryNormalizePath(Environment.GetEnvironmentVariable(WorkspaceEnvVar), out var fromSingleWorkspace))
        {
            return EnsureWorkspace(fromSingleWorkspace);
        }

        var configured = ParseWorkspaceList(Environment.GetEnvironmentVariable(WorkspacesEnvVar));
        if (configured.Count > 0)
        {
            return EnsureWorkspace(configured[0]);
        }

        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrWhiteSpace(documents))
        {
            throw new InvalidOperationException("Unable to resolve My Documents folder for MFA root.");
        }

        var existing = EnumerateExistingWorkspaceCandidates(documents)
            .FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(existing))
        {
            return EnsureWorkspace(existing);
        }

        return EnsureWorkspace(Path.Combine(documents, PrimaryWorkspaceName));
    }

    public static IReadOnlyList<string> ResolveWorkspaceRoots(int requestedCount)
    {
        var desiredCount = Math.Max(1, requestedCount);

        var configured = ParseWorkspaceList(Environment.GetEnvironmentVariable(WorkspacesEnvVar));
        if (configured.Count > 0)
        {
            return configured
                .Select(EnsureWorkspace)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrWhiteSpace(documents))
        {
            return Array.Empty<string>();
        }

        var roots = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var existing in EnumerateExistingWorkspaceCandidates(documents))
        {
            var canonical = CanonicalizeWorkspacePath(existing);
            if (seen.Add(canonical))
            {
                roots.Add(canonical);
            }
        }

        var generatedTarget = desiredCount <= 1 ? 1 : Math.Max(8, desiredCount);
        for (int i = 1; i <= generatedTarget; i++)
        {
            var generated = CanonicalizeWorkspacePath(Path.Combine(documents, $"MFA_{i}"));
            if (seen.Add(generated))
            {
                roots.Add(generated);
            }
        }

        if (roots.Count == 0)
        {
            roots.Add(Path.Combine(documents, PrimaryWorkspaceName));
        }

        return roots
            .Take(generatedTarget)
            .Select(EnsureWorkspace)
            .ToList();
    }

    private static IReadOnlyList<string> ParseWorkspaceList(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<string>();
        }

        var workspaces = new List<string>();
        foreach (var entry in raw.Split(Path.PathSeparator,
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (TryNormalizePath(entry, out var normalized))
            {
                workspaces.Add(normalized);
            }
        }

        return workspaces;
    }

    private static IEnumerable<string> EnumerateExistingWorkspaceCandidates(string documentsRoot)
    {
        IEnumerable<string> directories;
        try
        {
            directories = Directory.Exists(documentsRoot)
                ? Directory.EnumerateDirectories(documentsRoot, "MFA*",
                    SearchOption.TopDirectoryOnly)
                : Enumerable.Empty<string>();
        }
        catch
        {
            directories = Enumerable.Empty<string>();
        }

        return directories
            .Where(path =>
            {
                var name = Path.GetFileName(path);
                return name.Equals("MFA", StringComparison.OrdinalIgnoreCase)
                       || name.StartsWith("MFA_", StringComparison.OrdinalIgnoreCase);
            })
            .OrderBy(path => ExtractNumericSuffixOrder(Path.GetFileName(path)))
            .ThenBy(path => path, StringComparer.OrdinalIgnoreCase);
    }

    private static int ExtractNumericSuffixOrder(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return int.MaxValue;
        }

        if (name.Equals("MFA", StringComparison.OrdinalIgnoreCase))
        {
            return int.MaxValue - 1;
        }

        if (name.StartsWith("MFA_", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(name.AsSpan(4), out var parsed))
        {
            return parsed;
        }

        return int.MaxValue;
    }

    private static bool TryNormalizePath(string? path, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            normalized = CanonicalizeWorkspacePath(Path.GetFullPath(path.Trim()));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string EnsureWorkspace(string path)
    {
        var normalized = CanonicalizeWorkspacePath(path);
        SeedPrimaryWorkspaceFromLegacy(normalized);
        Directory.CreateDirectory(normalized);
        return Path.GetFullPath(normalized);
    }

    private static string CanonicalizeWorkspacePath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var name = Path.GetFileName(fullPath);
        if (!name.Equals(LegacyWorkspaceName, StringComparison.OrdinalIgnoreCase))
        {
            return fullPath;
        }

        var parent = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrWhiteSpace(parent))
        {
            return fullPath;
        }

        return Path.Combine(parent, PrimaryWorkspaceName);
    }

    private static void SeedPrimaryWorkspaceFromLegacy(string canonicalPath)
    {
        var name = Path.GetFileName(canonicalPath);
        if (!name.Equals(PrimaryWorkspaceName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var parent = Path.GetDirectoryName(canonicalPath);
        if (string.IsNullOrWhiteSpace(parent))
        {
            return;
        }

        var legacyPath = Path.Combine(parent, LegacyWorkspaceName);
        if (!Directory.Exists(legacyPath))
        {
            return;
        }

        Directory.CreateDirectory(canonicalPath);
        CopyFileIfMissing(
            Path.Combine(legacyPath, "global_config.yaml"),
            Path.Combine(canonicalPath, "global_config.yaml"));
        CopyDirectoryContentsIfMissing(
            Path.Combine(legacyPath, "pretrained_models"),
            Path.Combine(canonicalPath, "pretrained_models"));
    }

    private static void CopyFileIfMissing(string sourcePath, string destinationPath)
    {
        if (!File.Exists(sourcePath) || File.Exists(destinationPath))
        {
            return;
        }

        var destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        File.Copy(sourcePath, destinationPath, overwrite: false);
    }

    private static void CopyDirectoryContentsIfMissing(string sourceDirectory, string destinationDirectory)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            return;
        }

        Directory.CreateDirectory(destinationDirectory);

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(destinationDirectory, relative));
        }

        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, file);
            var destinationPath = Path.Combine(destinationDirectory, relative);
            if (File.Exists(destinationPath))
            {
                continue;
            }

            var destinationParent = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationParent))
            {
                Directory.CreateDirectory(destinationParent);
            }

            File.Copy(file, destinationPath, overwrite: false);
        }
    }
}
