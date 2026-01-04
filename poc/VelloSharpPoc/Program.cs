using Avalonia;
using VelloSharp.Avalonia.Vello;

namespace VelloSharpPoc;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseVello()   // Vello GPU rendering
            .WithInterFont()
            .LogToTrace();
}
