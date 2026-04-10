using System.Text.Json.Serialization;

namespace Ams.Core.Application.Runs;

public enum RunArtifactKind
{
    Input = 0,
    Output = 1,
    Intermediate = 2,
    Report = 3,
    Log = 4
}

public sealed record RunArtifact
{
    [JsonConstructor]
    public RunArtifact(string name, RunArtifactKind kind, string path, bool exists)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        Name = name;
        Kind = kind;
        Path = path;
        Exists = exists;
    }

    public string Name { get; }

    public RunArtifactKind Kind { get; }

    public string Path { get; }

    public bool Exists { get; }
}
