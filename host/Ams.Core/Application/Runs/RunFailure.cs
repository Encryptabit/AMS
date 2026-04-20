using System.Text.Json.Serialization;

namespace Ams.Core.Application.Runs;

public enum RunFailureKind
{
    Validation = 0,
    Dependency = 1,
    Timeout = 2,
    Cancelled = 3,
    Execution = 4
}

public sealed record RunFailure
{
    [JsonConstructor]
    public RunFailure(RunFailureKind kind, string message, string? stage = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (stage is not null && string.IsNullOrWhiteSpace(stage))
        {
            throw new ArgumentException("Stage cannot be blank when provided.", nameof(stage));
        }

        Kind = kind;
        Message = message;
        Stage = stage;
    }

    public RunFailureKind Kind { get; }

    public string Message { get; }

    public string? Stage { get; }
}
