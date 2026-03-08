using Ams.Core.Application.Mfa;

namespace Ams.Tests;

[CollectionDefinition("MfaWorkspaceResolver", DisableParallelization = true)]
public sealed class MfaWorkspaceResolverCollectionDefinition;

[Collection("MfaWorkspaceResolver")]
public sealed class MfaWorkspaceResolverTests : IDisposable
{
    private readonly string? _originalMfaRoot = Environment.GetEnvironmentVariable("MFA_ROOT_DIR");
    private readonly string? _originalWorkspace = Environment.GetEnvironmentVariable("AMS_MFA_WORKSPACE");
    private readonly string? _originalWorkspaces = Environment.GetEnvironmentVariable("AMS_MFA_WORKSPACES");
    private readonly List<string> _tempDirectories = new();

    public MfaWorkspaceResolverTests()
    {
        ClearWorkspaceEnvironment();
    }

    [Fact]
    public void ResolvePreferredRoot_LegacyMfaOverride_SeedsPrimaryWorkspace()
    {
        var root = CreateTempDirectory();
        var legacy = Path.Combine(root, "MFA");
        var acousticDir = Path.Combine(legacy, "pretrained_models", "acoustic");
        Directory.CreateDirectory(acousticDir);
        File.WriteAllText(Path.Combine(legacy, "global_config.yaml"), "profile: legacy");
        File.WriteAllText(Path.Combine(acousticDir, "english_mfa.zip"), "model");

        var resolved = MfaWorkspaceResolver.ResolvePreferredRoot(legacy);

        var primary = Path.Combine(root, "MFA_1");
        Assert.Equal(primary, resolved);
        Assert.True(File.Exists(Path.Combine(primary, "global_config.yaml")));
        Assert.True(File.Exists(Path.Combine(primary, "pretrained_models", "acoustic", "english_mfa.zip")));
    }

    [Fact]
    public void ResolveWorkspaceRoots_ConfiguredLegacyWorkspace_NormalizesToPrimaryWithoutDuplicates()
    {
        var root = CreateTempDirectory();
        var legacy = Path.Combine(root, "MFA");
        var second = Path.Combine(root, "MFA_2");
        Environment.SetEnvironmentVariable(
            "AMS_MFA_WORKSPACES",
            string.Join(Path.PathSeparator, legacy, second));

        var roots = MfaWorkspaceResolver.ResolveWorkspaceRoots(2);

        Assert.Equal(
            new[]
            {
                Path.Combine(root, "MFA_1"),
                second
            },
            roots);
    }

    public void Dispose()
    {
        RestoreEnvironmentVariable("MFA_ROOT_DIR", _originalMfaRoot);
        RestoreEnvironmentVariable("AMS_MFA_WORKSPACE", _originalWorkspace);
        RestoreEnvironmentVariable("AMS_MFA_WORKSPACES", _originalWorkspaces);

        foreach (var directory in _tempDirectories)
        {
            try
            {
                Directory.Delete(directory, recursive: true);
            }
            catch
            {
                // Best-effort cleanup.
            }
        }
    }

    private static void RestoreEnvironmentVariable(string name, string? value)
    {
        Environment.SetEnvironmentVariable(name, value);
    }

    private static void ClearWorkspaceEnvironment()
    {
        Environment.SetEnvironmentVariable("MFA_ROOT_DIR", null);
        Environment.SetEnvironmentVariable("AMS_MFA_WORKSPACE", null);
        Environment.SetEnvironmentVariable("AMS_MFA_WORKSPACES", null);
    }

    private string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ams-mfa-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        _tempDirectories.Add(path);
        return path;
    }
}
