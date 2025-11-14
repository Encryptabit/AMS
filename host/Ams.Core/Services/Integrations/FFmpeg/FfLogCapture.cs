using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace Ams.Core.Services.Integrations.FFmpeg;

internal static unsafe class FfLogCapture
{
    private static readonly object Sync = new();
    private static readonly AvLogDelegate LogDelegate = LogCallback;
    private static readonly IntPtr CallbackPtr = Marshal.GetFunctionPointerForDelegate(LogDelegate);
    private static readonly av_log_set_callback_callback_func Callback = new() { Pointer = CallbackPtr };
    [ThreadStatic] private static List<string>? _current;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate void AvLogDelegate(void* ptr, int level, string format, byte* vl);

    public static List<string> Capture(Action action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        FfSession.EnsureFiltersAvailable();

        lock (Sync)
        {
            var collector = new List<string>(32);
            _current = collector;
            ffmpeg.av_log_set_callback(Callback);
            int previousLevel = ffmpeg.av_log_get_level();
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_INFO);

            try
            {
                action();
            }
            finally
            {
                ffmpeg.av_log_set_level(previousLevel);
                ffmpeg.av_log_set_callback(null);
                _current = null;
            }

            return collector;
        }
    }

    private static unsafe void LogCallback(void* ptr, int level, string format, byte* vl)
    {
        var collector = _current;
        if (collector == null)
        {
            return;
        }

        const int bufferSize = 1024;
        byte* buffer = stackalloc byte[bufferSize];
        int printPrefix = 1;
        ffmpeg.av_log_format_line(ptr, level, format, vl, buffer, bufferSize, &printPrefix);
        var line = Marshal.PtrToStringAnsi((IntPtr)buffer);
        if (!string.IsNullOrWhiteSpace(line))
        {
            collector.Add(line.Trim());
        }
    }
}
