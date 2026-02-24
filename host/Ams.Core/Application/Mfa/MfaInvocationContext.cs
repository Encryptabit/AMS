using System.Threading;

namespace Ams.Core.Application.Mfa;

internal static class MfaInvocationContext
{
    private static readonly AsyncLocal<string?> CurrentLabel = new();
    private static readonly IDisposable NoopScope = new NoopDisposable();

    public static string? Label => CurrentLabel.Value;

    public static IDisposable BeginScope(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return NoopScope;
        }

        var previous = CurrentLabel.Value;
        CurrentLabel.Value = label.Trim();
        return new RestoreScope(previous);
    }

    private sealed class RestoreScope : IDisposable
    {
        private readonly string? _previous;
        private bool _disposed;

        public RestoreScope(string? previous)
        {
            _previous = previous;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            CurrentLabel.Value = _previous;
            _disposed = true;
        }
    }

    private sealed class NoopDisposable : IDisposable
    {
        public void Dispose()
        {
            // Intentionally empty.
        }
    }
}
