namespace Ams.Web.Utilities;

internal static class AppDataPaths
{
    private static readonly Lazy<string> Root = new(() =>
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrEmpty(basePath))
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        if (string.IsNullOrEmpty(basePath))
        {
            basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ams");
        }

        var target = Path.Combine(basePath, "AMS");
        Directory.CreateDirectory(target);
        return target;
    });

    public static string Resolve(string fileName) => Path.Combine(Root.Value, fileName);
}
