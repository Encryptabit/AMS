namespace Ams.Core.Runtime.Common;

internal sealed class DocumentSlotOptions<T>
    where T : class
{
    public Func<T?, T?>? PostLoadTransform { get; init; }
    public Func<FileInfo?>? BackingFileAccessor { get; init; }
    public bool WriteThrough { get; init; }
}