namespace Ams.Cli.Services;

internal enum DspOutputMode
{
    Post,
    Source
}

internal static class DspSessionState
{
    public static DspOutputMode OutputMode { get; set; } = DspOutputMode.Post;

    public static bool OverwriteOutputs { get; set; }
}