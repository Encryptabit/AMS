using System.Globalization;

namespace Ams.Core.Common;

public static class NaturalStringComparer
{
    private static readonly StringComparer NumericIgnoreCaseComparer =
        StringComparer.Create(
            CultureInfo.InvariantCulture,
            CompareOptions.NumericOrdering | CompareOptions.IgnoreCase);

    public static IComparer<string> SortIgnoreCase { get; } =
        new NaturalSortComparer(NumericIgnoreCaseComparer, StringComparer.OrdinalIgnoreCase);

    public static IComparer<FileInfo> FileNameWithoutExtensionIgnoreCase { get; } =
        new FileNameWithoutExtensionComparer();

    public static int CompareIgnoreCase(string? x, string? y)
        => SortIgnoreCase.Compare(x, y);

    public static int CompareFileNameWithoutExtension(FileInfo? x, FileInfo? y)
        => FileNameWithoutExtensionIgnoreCase.Compare(x, y);

    private sealed class NaturalSortComparer(
        StringComparer numericComparer,
        StringComparer ordinalIgnoreCaseComparer) : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            var numeric = numericComparer.Compare(x, y);
            if (numeric != 0)
            {
                return numeric;
            }

            var ordinalIgnoreCase = ordinalIgnoreCaseComparer.Compare(x, y);
            if (ordinalIgnoreCase != 0)
            {
                return ordinalIgnoreCase;
            }

            return StringComparer.Ordinal.Compare(x, y);
        }
    }

    private sealed class FileNameWithoutExtensionComparer : IComparer<FileInfo>
    {
        public int Compare(FileInfo? x, FileInfo? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            var stem = CompareIgnoreCase(
                Path.GetFileNameWithoutExtension(x.Name),
                Path.GetFileNameWithoutExtension(y.Name));
            if (stem != 0)
            {
                return stem;
            }

            var fileName = CompareIgnoreCase(x.Name, y.Name);
            if (fileName != 0)
            {
                return fileName;
            }

            return StringComparer.Ordinal.Compare(x.FullName, y.FullName);
        }
    }
}
