using System;

namespace Ams.Core.Services.Integrations.FFmpeg;

/// <summary>
/// Lifetime wrapper for FFmpeg global init/teardown.
/// </summary>
public sealed class FfSession : IDisposable
{
    public void Dispose()
    {
        // Placeholder until FFmpeg.AutoGen is wired up.
    }
}
